using Alexordle.Client.Application.Database;
using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Database.Models;
using Alexordle.Client.Application.Exceptions;
using Lexicom.DependencyInjection.Primitives;
using Microsoft.EntityFrameworkCore;

namespace Alexordle.Client.Application.Services;
public interface IPuzzleService
{
    Task<bool> PuzzleExistsAsync(Guid puzzleId);
    /// <exception cref="PuzzleDoesNotExistException"></exception>
    Task<Puzzle> GetPuzzleAsync(Guid puzzleId);
    Task<Puzzle> StartPuzzleAsync(int width, int? maxGuesses, bool isSpellChecking, IEnumerable<string> clues, IEnumerable<string> answers);
}
public class PuzzleService : IPuzzleService
{
    private readonly IDbContextFactory<AlexordleDbContext> _dbContextFactory;
    private readonly IGuidProvider _guidProvider;
    private readonly IHintService _hintService;

    public PuzzleService(
        IDbContextFactory<AlexordleDbContext> dbContextFactory,
        IGuidProvider guidProvider,
        IHintService hintService)
    {
        _dbContextFactory = dbContextFactory;
        _guidProvider = guidProvider;
        _hintService = hintService;
    }

    public async Task<bool> PuzzleExistsAsync(Guid puzzleId)
    {
        Puzzle? puzzle = await GetPuzzleOrDefaultAsync(puzzleId);

        return puzzle is not null;
    }

    public async Task<Puzzle> GetPuzzleAsync(Guid puzzleId)
    {
        Puzzle? puzzle = await GetPuzzleOrDefaultAsync(puzzleId);

        if (puzzle is null)
        {
            throw new PuzzleDoesNotExistException(puzzleId);
        }

        return puzzle;
    }

    public async Task<Puzzle> StartPuzzleAsync(int width, int? maxGuesses, bool isSpellChecking, IEnumerable<string> clues, IEnumerable<string> answers)
    {
        if (width is < Puzzle.VALIDATION_WIDTH_MINIMUM)
        {
            throw new ArgumentLessThanException(Puzzle.VALIDATION_WIDTH_MINIMUM, width);
        }

        if (width is > Puzzle.VALIDATION_WIDTH_MAXIMUM)
        {
            throw new ArgumentGreaterThanException(Puzzle.VALIDATION_WIDTH_MAXIMUM, width);
        }

        if (maxGuesses is not null)
        {
            if (maxGuesses.Value is < Puzzle.VALIDATION_MAXGUESSES_MINIMUM)
            {
                throw new ArgumentLessThanException(Puzzle.VALIDATION_MAXGUESSES_MINIMUM, width);
            }

            if (maxGuesses.Value is > Puzzle.VALIDATION_MAXGUESSES_MAXIMUM)
            {
                throw new ArgumentGreaterThanException(Puzzle.VALIDATION_MAXGUESSES_MAXIMUM, width);
            }
        }

        var answersList = answers.ToList();

        if (answersList.Count is < AnswerCharacter.VALIDATION_COUNT_MINIMUM)
        {
            throw new ArgumentLessThanException(AnswerCharacter.VALIDATION_COUNT_MINIMUM, answersList.Count);
        }

        if (answersList.Count is > AnswerCharacter.VALIDATION_COUNT_MAXIMUM)
        {
            throw new ArgumentGreaterThanException(AnswerCharacter.VALIDATION_COUNT_MAXIMUM, answersList.Count);
        }

        var cluesList = clues.ToList();

        var puzzle = new Puzzle
        {
            Id = _guidProvider.NewGuid(),
            Width = width,
            IsSpellChecking = isSpellChecking,
            TotalAnswers = answersList.Count,
            CurrentAnswers = 0,
            MaxGuesses = maxGuesses,
            CurrentGuesses = 0,
            IsFinished = false,
        };

        using var db = await _dbContextFactory.CreateDbContextAsync();

        try
        {
            foreach (string answerText in answersList)
            {
                string invariantText = answerText.ToUpperInvariant();

                if (invariantText.Length != puzzle.Width)
                {
                    throw new WidthMismatchException(invariantText, invariantText.Length, puzzle.Width);
                }

                if (answersList.Count(ua => string.Equals(ua, answerText, StringComparison.OrdinalIgnoreCase)) is > 1)
                {
                    throw new DuplicateAnswerException(answerText);
                }

                Guid answerId = _guidProvider.NewGuid();

                for (int column = 0; column < invariantText.Length; column++)
                {
                    char invariantCharacter = invariantText[column];

                    if (!AnswerCharacter.VALIDATION_CHARACTERS_SUPPORTED.Contains(invariantCharacter))
                    {
                        throw new AnswerCharacterNotSupportedException(invariantCharacter, invariantText);
                    }

                    bool anotherAnswerContainsCharacter = await db.AnswerCharacters
                        .AsNoTracking()
                        .AnyAsync(ac => ac.AnswerId != answerId && ac.InvariantCharacter == invariantCharacter);

                    if (anotherAnswerContainsCharacter)
                    {
                        throw new DuplicateAnswerCharacterException(invariantCharacter);
                    }

                    await db.AnswerCharacters.AddAsync(new AnswerCharacter
                    {
                        PuzzleId = puzzle.Id,
                        AnswerId = answerId,
                        Column = column,
                        InvariantCharacter = invariantCharacter,
                    });

                    await db.SaveChangesAsync();
                }

                await db.Answers.AddAsync(new Answer
                {
                    Id = answerId,
                    PuzzleId = puzzle.Id,
                    InvariantText = invariantText,
                    IsSolved = false,
                });
            }

            for (int row = 0; row < cluesList.Count; row++)
            {
                string invariantText = cluesList[row].ToUpperInvariant();

                if (invariantText.Length != puzzle.Width)
                {
                    throw new WidthMismatchException(invariantText, invariantText.Length, puzzle.Width);
                }

                Guid clueId = _guidProvider.NewGuid();

                var createdClues = new List<ClueCharacter>();
                for (int column = 0; column < invariantText.Length; column++)
                {
                    char invariantCharacter = invariantText[column];

                    if (!ClueCharacter.VALIDATION_CHARACTERS_SUPPORTED.Contains(invariantCharacter))
                    {
                        throw new ClueCharacterNotSupportedException(invariantCharacter, invariantText);
                    }

                    var clue = new ClueCharacter
                    {
                        PuzzleId = puzzle.Id,
                        ClueId = clueId,
                        Row = row,
                        Column = column,
                        InvariantCharacter = invariantCharacter,
                        Hint = Hints.None,
                    };

                    await _hintService.CalculateHintAsync(clue);

                    await db.ClueCharacters.AddAsync(clue);

                    await db.SaveChangesAsync();

                    createdClues.Add(clue);
                }

                await _hintService.PostCalculateHintsAsync(createdClues);

                await db.Clues.AddAsync(new Clue
                {
                    Id = clueId,
                    PuzzleId = puzzle.Id,
                    InvariantText = invariantText,
                });
            }

            await db.Hunches.AddAsync(new Hunch
            {
                PuzzleId = puzzle.Id,
                IsBonus = false,
                IsInfinite = maxGuesses is null,
            });

            await db.Puzzles.AddAsync(puzzle);

            await db.SaveChangesAsync();

            return puzzle;
        }
        catch
        {
            List<AnswerCharacter> answersToRemove = await db.AnswerCharacters
                .AsNoTracking()
                .Where(a => a.PuzzleId == puzzle.Id)
                .ToListAsync();
            db.AnswerCharacters.RemoveRange(answersToRemove);

            List<ClueCharacter> cluesToRemove = await db.ClueCharacters
                .AsNoTracking()
                .Where(c => c.PuzzleId == puzzle.Id)
                .ToListAsync();
            db.ClueCharacters.RemoveRange(cluesToRemove);

            Puzzle? puzzleToRemove = await db.Puzzles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == puzzle.Id);

            if (puzzleToRemove is not null)
            {
                db.Puzzles.Remove(puzzleToRemove);
            }

            await db.SaveChangesAsync();

            throw;
        }
    }

    private async Task<Puzzle?> GetPuzzleOrDefaultAsync(Guid puzzleId)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        return await db.Puzzles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == puzzleId);
    }
}
