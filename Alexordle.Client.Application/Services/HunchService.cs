using Alexordle.Client.Application.Database;
using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Database.Models;
using Alexordle.Client.Application.Exceptions;
using Lexicom.DependencyInjection.Primitives;
using Microsoft.EntityFrameworkCore;

namespace Alexordle.Client.Application.Services;
public interface IHunchService
{
    /// <exception cref="HunchDoesNotExistException"></exception>
    Task<Hunch> GetHunchAsync(Guid puzzleId);
    Task<IReadOnlyList<HunchCharacter>> GetHunchesAsync(Guid puzzleId);
    /// <exception cref="HunchCharacterNotWhitelistedException"></exception>
    /// <exception cref="PuzzleDoesNotExistException"></exception>
    Task AppendCharacterToHunchAsync(Guid puzzleId, char character);
    Task RemoveCharacterFromHunchAsync(Guid puzzleId);
    /// <exception cref="PuzzleDoesNotExistException"></exception>
    /// <exception cref="IncompleteGuessException"></exception>
    /// <exception cref="IncorrectSpellingException"></exception>
    /// <exception cref="HunchDoesNotExistException"></exception>
    Task SubmitDesignHunchAsync(Guid puzzleId);
    /// <exception cref="PuzzleDoesNotExistException"></exception>
    /// <exception cref="IncompleteGuessException"></exception>
    /// <exception cref="IncorrectSpellingException"></exception>
    /// <exception cref="DuplicateGuessException"></exception>
    /// <exception cref="HunchDoesNotExistException"></exception>
    Task SubmitHunchAsync(Guid puzzleId);
}
public class HunchService : IHunchService
{
    private readonly IDbContextFactory<AlexordleDbContext> _dbContextFactory;
    private readonly IDictionaryService _dictionaryService;
    private readonly IGuessService _guessService;
    private readonly IHintService _hintService;
    private readonly IGuidProvider _guidProvider;

    public HunchService(
        IDbContextFactory<AlexordleDbContext> dbContextFactory,
        IDictionaryService dictionaryService,
        IGuessService guessService,
        IHintService hintService,
        IGuidProvider guidProvider)
    {
        _dbContextFactory = dbContextFactory;
        _dictionaryService = dictionaryService;
        _guessService = guessService;
        _hintService = hintService;
        _guidProvider = guidProvider;
    }

    public async Task<Hunch> GetHunchAsync(Guid puzzleId)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        Hunch? hunch = await db.Hunches
             .AsNoTracking()
             .FirstOrDefaultAsync(h => h.PuzzleId == puzzleId);

        if (hunch is null)
        {
            throw new HunchDoesNotExistException(puzzleId);
        }

        return hunch;
    }

    public async Task<IReadOnlyList<HunchCharacter>> GetHunchesAsync(Guid puzzleId)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        return await db.HunchCharacters
             .AsNoTracking()
             .Where(c => c.PuzzleId == puzzleId)
             .ToListAsync();
    }

    public async Task AppendCharacterToHunchAsync(Guid puzzleId, char character)
    {
        char invariantCharacter = char.ToUpperInvariant(character);
        if (!AnswerCharacter.VALIDATION_CHARACTERS_WHITELIST.Contains(invariantCharacter))
        {
            throw new HunchCharacterNotWhitelistedException(invariantCharacter);
        }

        using var db = await _dbContextFactory.CreateDbContextAsync();

        Puzzle? puzzle = await db.Puzzles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == puzzleId);

        if (puzzle is null)
        {
            throw new PuzzleDoesNotExistException(puzzleId);
        }

        int currentLength = await db.HunchCharacters
             .AsNoTracking()
             .CountAsync(h => h.PuzzleId == puzzleId);

        if (currentLength >= puzzle.Width)
        {
            return;
        }

        var hunch = new HunchCharacter
        {
            PuzzleId = puzzle.Id,
            Column = currentLength,
            InvariantCharacter = invariantCharacter,
            Hint = Hints.None,
        };

        await _hintService.CalculateHintAsync(hunch);

        await db.HunchCharacters.AddAsync(hunch);

        await db.SaveChangesAsync();
    }

    public async Task RemoveCharacterFromHunchAsync(Guid puzzleId)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        HunchCharacter? lastHunch = await db.HunchCharacters
            .AsNoTracking()
            .Where(h => h.PuzzleId == puzzleId)
            .OrderByDescending(h => h.Column)
            .FirstOrDefaultAsync();

        if (lastHunch is null)
        {
            return;
        }

        db.HunchCharacters.Remove(lastHunch);

        await db.SaveChangesAsync();
    }

    public async Task SubmitDesignHunchAsync(Guid puzzleId)
    {
        await SubmitHunchAsync(isDesign: true, puzzleId);
    }

    public async Task SubmitHunchAsync(Guid puzzleId)
    {
        await SubmitHunchAsync(isDesign: false, puzzleId);
    }

    private async Task SubmitHunchAsync(bool isDesign, Guid puzzleId)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        Puzzle? puzzle = await db.Puzzles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == puzzleId);

        if (puzzle is null)
        {
            throw new PuzzleDoesNotExistException(puzzleId);
        }

        Hunch? hunch = await db.Hunches
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.PuzzleId == puzzle.Id);

        if (hunch is null)
        {
            throw new HunchDoesNotExistException(puzzle.Id);
        }

        List<HunchCharacter> hunches = await db.HunchCharacters
            .AsNoTracking()
            .Where(h => h.PuzzleId == puzzleId)
            .OrderBy(h => h.Column)
            .ToListAsync();

        if (hunches.Count < puzzle.Width)
        {
            throw new IncompleteGuessException(hunches.Count, puzzle.Width);
        }

        int? lastRow = await db.GuessCharacters
            .AsNoTracking()
            .Where(g => g.PuzzleId == puzzleId)
            .MaxAsync(g => (int?)g.Row);

        int row = (lastRow ?? -1) + 1;

        Guid guessId = _guidProvider.NewGuid();
        string guessInvariantText = string.Empty;
        bool isHuh = true;

        var guesses = new List<GuessCharacter>();
        foreach (HunchCharacter h in hunches)
        {
            var guess = new GuessCharacter
            {
                PuzzleId = puzzle.Id,
                GuessId = guessId,
                Row = row,
                Column = h.Column,
                InvariantCharacter = h.InvariantCharacter,
                Hint = Hints.None,
            };

            await _hintService.CalculateHintAsync(guess);

            guesses.Add(guess);

            guessInvariantText += h.InvariantCharacter;

            if (guess.Hint is not Hints.Correct)
            {
                isHuh = false;
            }
        }

        bool isAnswer = false;

        Answer? answer = await db.Answers
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.PuzzleId == puzzleId && a.InvariantText == guessInvariantText);

        if (answer is not null)
        {
            puzzle.CurrentAnswers++;
            answer.IsSolved = true;

            db.Answers.Update(answer);

            isAnswer = true;
            isHuh = false;

            if (isDesign)
            {
                await _hintService.PostCalculateDesignAnswerHints(answer.Id, guesses);
            }
        }

        if (!isDesign && !isAnswer && puzzle.IsSpellChecking)
        {
            bool isSpelledCorrectly = await _dictionaryService.IsSpelledCorrectlyAsync(puzzleId, guessInvariantText);
            if (!isSpelledCorrectly)
            {
                throw new IncorrectSpellingException(guessInvariantText);
            }
        }

        IReadOnlyList<string> guessInvariantTexts = await _guessService.GetGuessInvariantTextsAsync(puzzleId);

        if (!isDesign && guessInvariantTexts.Contains(guessInvariantText))
        {
            throw new DuplicateGuessException(guessInvariantText);
        }

        await _hintService.PostCalculateHintsAsync(puzzleId, guesses);

        puzzle.CurrentGuesses++;

        bool defeat = puzzle.MaxGuesses is not null && puzzle.CurrentGuesses >= puzzle.MaxGuesses;
        bool victory = puzzle.TotalAnswers - puzzle.CurrentAnswers is <= 0;

        bool isBonus = hunch.IsBonus;
        if (isAnswer && defeat)
        {
            defeat = false;
            hunch.IsBonus = true;

            db.Hunches.Update(hunch);
        }

        puzzle.IsFinished = victory || defeat;

        db.Puzzles.Update(puzzle);

        await db.Guesses.AddAsync(new Guess
        {
            Id = guessId,
            PuzzleId = puzzleId,
            Row = row,
            InvariantText = guessInvariantText,
            IsAnswer = isAnswer,
            IsBonus = isBonus,
            IsHuh = isHuh,
        });

        await db.GuessCharacters.AddRangeAsync(guesses);

        db.RemoveRange(hunches);

        await db.SaveChangesAsync();
    }
}
