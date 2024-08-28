using Alexordle.Client.Application.Database;
using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Database.Models;
using Alexordle.Client.Application.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Alexordle.Client.Application.Services;
public interface IHunchService
{
    Task<IReadOnlyList<Hunch>> GetHunchesAsync(Guid puzzleId);
    /// <exception cref="HunchCharacterNotSupportedException"></exception>
    /// <exception cref="PuzzleDoesNotExistException"></exception>
    Task AppendCharacterToHunchAsync(Guid puzzleId, char character);
    Task RemoveCharacterFromHunchAsync(Guid puzzleId);
    /// <exception cref="PuzzleDoesNotExistException"></exception>
    /// <exception cref="IncompleteGuessException"></exception>
    /// <exception cref="IncorrectSpellingException"></exception>
    /// <exception cref="DuplicateGuessException"></exception>
    Task SubmitHunchAsync(Guid puzzleId);
}
public class HunchService : IHunchService
{
    private readonly IDbContextFactory<AlexordleDbContext> _dbContextFactory;
    private readonly IDictionaryService _dictionaryService;
    private readonly IGuessService _guessService;
    private readonly IHintService _hintService;

    public HunchService(
        IDbContextFactory<AlexordleDbContext> dbContextFactory,
        IDictionaryService dictionaryService,
        IGuessService guessService,
        IHintService hintService)
    {
        _dbContextFactory = dbContextFactory;
        _dictionaryService = dictionaryService;
        _guessService = guessService;
        _hintService = hintService;
    }

    public async Task<IReadOnlyList<Hunch>> GetHunchesAsync(Guid puzzleId)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        return await db.Hunches
             .AsNoTracking()
             .Where(c => c.PuzzleId == puzzleId)
             .ToListAsync();
    }

    public async Task AppendCharacterToHunchAsync(Guid puzzleId, char character)
    {
        char invariantCharacter = char.ToUpperInvariant(character);
        if (!Answer.VALIDATION_CHARACTERS_SUPPORTED.Contains(character))
        {
            throw new HunchCharacterNotSupportedException(invariantCharacter);
        }

        using var db = await _dbContextFactory.CreateDbContextAsync();

        Puzzle? puzzle = await db.Puzzles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == puzzleId);

        if (puzzle is null)
        {
            throw new PuzzleDoesNotExistException(puzzleId);
        }

        int currentLength = await db.Hunches
             .AsNoTracking()
             .CountAsync(h => h.PuzzleId == puzzleId);

        if (currentLength >= puzzle.Width)
        {
            return;
        }

        var hunch = new Hunch
        {
            PuzzleId = puzzle.Id,
            Column = currentLength,
            InvariantCharacter = invariantCharacter,
            Hint = Hints.None,
        };

        await _hintService.CalculateHintAsync(hunch);

        await db.Hunches.AddAsync(hunch);

        await db.SaveChangesAsync();
    }

    public async Task RemoveCharacterFromHunchAsync(Guid puzzleId)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        Hunch? lastHunch = await db.Hunches
            .AsNoTracking()
            .Where(h => h.PuzzleId == puzzleId)
            .OrderByDescending(h => h.Column)
            .FirstOrDefaultAsync();

        if (lastHunch is null)
        {
            return;
        }

        db.Hunches.Remove(lastHunch);

        await db.SaveChangesAsync();
    }

    public async Task SubmitHunchAsync(Guid puzzleId)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        Puzzle? puzzle = await db.Puzzles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == puzzleId);

        if (puzzle is null)
        {
            throw new PuzzleDoesNotExistException(puzzleId);
        }

        List<Hunch> hunches = await db.Hunches
            .AsNoTracking()
            .Where(h => h.PuzzleId == puzzleId)
            .OrderBy(h => h.Column)
            .ToListAsync();

        if (hunches.Count < puzzle.Width)
        {
            throw new IncompleteGuessException(hunches.Count, puzzle.Width);
        }

        int? lastRow = await db.Guesses
            .AsNoTracking()
            .MaxAsync(g => (int?)g.Row);

        bool isAnswer = true;
        string? invariantText = puzzle.IsSpellChecking ? string.Empty : null;

        var guesses = new List<Guess>();
        foreach (Hunch h in hunches)
        {
            if (isAnswer)
            {
                isAnswer = await db.Answers.AnyAsync(a => a.Column == h.Column && a.InvariantCharacter == h.InvariantCharacter);
            }

            var guess = new Guess
            {
                PuzzleId = puzzle.Id,
                Row = (lastRow ?? -1) + 1,
                Column = h.Column,
                InvariantCharacter = h.InvariantCharacter,
                Hint = Hints.None,
            };

            await _hintService.CalculateHintAsync(guess);

            guesses.Add(guess);

            if (invariantText is not null)
            {
                invariantText += h.InvariantCharacter;
            }
        }

        if (!isAnswer && invariantText is not null)
        {
            bool isSpelledCorrectly = await _dictionaryService.IsSpelledCorrectlyAsync(puzzleId, invariantText);
            if (!isSpelledCorrectly)
            {
                throw new IncorrectSpellingException(invariantText);
            }
        }

        IReadOnlyList<string> guessInvariantTexts = await _guessService.GetGuessInvariantTextsAsync(puzzleId);

        if (guessInvariantTexts.Contains(invariantText))
        {
            throw new DuplicateGuessException(invariantText);
        }

        await _hintService.PostCalculateHintsAsync(guesses);

        puzzle.CurrentGuesses++;

        if (isAnswer)
        {
            puzzle.CurrentAnswers++;
        }

        puzzle.IsComplete = puzzle.TotalAnswers - puzzle.CurrentAnswers is <= 0 || puzzle.CurrentGuesses >= puzzle.MaxGuesses;

        db.Puzzles.Update(puzzle);

        await db.Guesses.AddRangeAsync(guesses);

        db.RemoveRange(hunches);

        await db.SaveChangesAsync();
    }
}
