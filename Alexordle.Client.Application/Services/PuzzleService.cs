using Alexordle.Client.Application.Database;
using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Database.Models;
using Alexordle.Client.Application.Exceptions;
using Alexordle.Client.Application.Models;
using Lexicom.DependencyInjection.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Alexordle.Client.Application.Services;
public interface IPuzzleService
{
    Task<bool> PuzzleExistsAsync(Guid puzzleId);
    /// <exception cref="PuzzleDoesNotExistException"></exception>
    Task<Puzzle> GetPuzzleAsync(Guid puzzleId);
    Task<DesignResult> CreateDesignAsync(int width, int? maxGuesses, bool isSpellChecking, IEnumerable<string> clues, IEnumerable<string> answers);
    Task<Puzzle> StartPuzzleAsync(int width, int? maxGuesses, bool isSpellChecking, IEnumerable<string> clues, IEnumerable<string> answers);
    Task DeletePuzzlesAsync();
}
public class PuzzleService : IPuzzleService
{
    private readonly ILogger<PuzzleService> _logger;
    private readonly IDbContextFactory<AlexordleDbContext> _dbContextFactory;
    private readonly IGuidProvider _guidProvider;
    private readonly IHintService _hintService;

    public PuzzleService(
        ILogger<PuzzleService> logger,
        IDbContextFactory<AlexordleDbContext> dbContextFactory,
        IGuidProvider guidProvider,
        IHintService hintService)
    {
        _logger = logger;
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

    public async Task<DesignResult> CreateDesignAsync(int width, int? maxGuesses, bool isSpellChecking, IEnumerable<string> clues, IEnumerable<string> answers)
    {
        (Puzzle _, DesignResult design) = await StartPuzzleAsync(isDesign: true, width, maxGuesses, isSpellChecking, clues, answers);

        return design;
    }

    public async Task<Puzzle> StartPuzzleAsync(int width, int? maxGuesses, bool isSpellChecking, IEnumerable<string> clues, IEnumerable<string> answers)
    {
        (Puzzle puzzle, DesignResult _) = await StartPuzzleAsync(isDesign: false, width, maxGuesses, isSpellChecking, clues, answers);

        return puzzle;
    }

    public async Task DeletePuzzlesAsync()
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        List<Puzzle> puzzles = await db.Puzzles
            .AsNoTracking()
            .ToListAsync();

        foreach (Puzzle puzzle in puzzles)
        {
            await DeletePuzzleAsync(db, puzzle.Id);
        }
    }

    private async Task DeletePuzzleAsync(AlexordleDbContext db, Guid puzzleId)
    {
        try
        {
            List<AnswerCharacter> answerCharactersToRemove = await db.AnswerCharacters
                .AsNoTracking()
                .Where(ac => ac.PuzzleId == puzzleId)
                .ToListAsync();
            db.AnswerCharacters.RemoveRange(answerCharactersToRemove);

            await db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete the answer characters for the puzzle with the id '{puzzleId}'", puzzleId);
        }

        try
        {
            List<Answer> answersToRemove = await db.Answers
                .AsNoTracking()
                .Where(a => a.PuzzleId == puzzleId)
                .ToListAsync();
            db.Answers.RemoveRange(answersToRemove);

            await db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete the answers for the puzzle with the id '{puzzleId}'", puzzleId);
        }

        try
        {
            List<ClueCharacter> clueCharactersToRemove = await db.ClueCharacters
                        .AsNoTracking()
                        .Where(cc => cc.PuzzleId == puzzleId)
                        .ToListAsync();
            db.ClueCharacters.RemoveRange(clueCharactersToRemove);

            await db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete the clue characters for the puzzle with the id '{puzzleId}'", puzzleId);
        }

        try
        {
            List<Clue> cluesToRemove = await db.Clues
                .AsNoTracking()
                .Where(cc => cc.PuzzleId == puzzleId)
                .ToListAsync();
            db.Clues.RemoveRange(cluesToRemove);

            await db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete the clues for the puzzle with the id '{puzzleId}'", puzzleId);
        }

        try
        {
            List<GuessCharacter> guessCharactersToRemove = await db.GuessCharacters
                .AsNoTracking()
                .Where(gc => gc.PuzzleId == puzzleId)
                .ToListAsync();
            db.GuessCharacters.RemoveRange(guessCharactersToRemove);

            await db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete the guess characters for the puzzle with the id '{puzzleId}'", puzzleId);
        }

        try
        {
            List<Guess> guessesToRemove = await db.Guesses
                .AsNoTracking()
                .Where(g => g.PuzzleId == puzzleId)
                .ToListAsync();
            db.Guesses.RemoveRange(guessesToRemove);

            await db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete the guesses for the puzzle with the id '{puzzleId}'", puzzleId);
        }

        try
        {
            List<HunchCharacter> hunchCharactersToRemove = await db.HunchCharacters
                .AsNoTracking()
                .Where(hc => hc.PuzzleId == puzzleId)
                .ToListAsync();
            db.HunchCharacters.RemoveRange(hunchCharactersToRemove);

            await db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete the hunch characters for the puzzle with the id '{puzzleId}'", puzzleId);
        }
        try
        {
            List<Hunch> hunchesToRemove = await db.Hunches
                .AsNoTracking()
                .Where(h => h.PuzzleId == puzzleId)
                .ToListAsync();
            db.Hunches.RemoveRange(hunchesToRemove);

            await db.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete the hunches for the puzzle with the id '{puzzleId}'", puzzleId);
        }

        try
        {
            Puzzle? puzzleToRemove = await db.Puzzles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == puzzleId);

            if (puzzleToRemove is not null)
            {
                db.Puzzles.Remove(puzzleToRemove);

                await db.SaveChangesAsync();
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete the puzzle with the id '{puzzleId}'", puzzleId);
        }
    }

    private async Task<Puzzle?> GetPuzzleOrDefaultAsync(Guid puzzleId)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        return await db.Puzzles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == puzzleId);
    }

    private async Task<(Puzzle puzzle, DesignResult design)> StartPuzzleAsync(bool isDesign, int width, int? maxGuesses, bool isSpellChecking, IEnumerable<string> clues, IEnumerable<string> answers)
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
                throw new ArgumentLessThanException(Puzzle.VALIDATION_MAXGUESSES_MINIMUM, maxGuesses);
            }

            if (maxGuesses.Value is > Puzzle.VALIDATION_MAXGUESSES_MAXIMUM)
            {
                throw new ArgumentGreaterThanException(Puzzle.VALIDATION_MAXGUESSES_MAXIMUM, maxGuesses);
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

        var design = new DesignResult
        {
            PuzzleId = puzzle.Id,
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
                    if (!isDesign)
                    {
                        throw new DuplicateAnswerException(answerText);
                    }
                    else
                    {
                        design.HasDuplicateAnswerCharacter = true;
                    }
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
                        .AnyAsync(ac => ac.PuzzleId == puzzle.Id && ac.AnswerId != answerId && ac.InvariantCharacter == invariantCharacter);

                    if (anotherAnswerContainsCharacter)
                    {
                        if (!isDesign)
                        {
                            throw new DuplicateAnswerCharacterException(invariantCharacter);
                        }
                        else
                        {
                            design.HasDuplicateAnswerCharacter = true;
                        }
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
                string initalInvariantText = cluesList[row].ToUpperInvariant();

                Guid clueId = _guidProvider.NewGuid();

                string invariantText = string.Empty;

                var createdClues = new List<ClueCharacter>();
                for (int column = 0; column < initalInvariantText.Length; column++)
                {
                    if (column < puzzle.Width)
                    {
                        char invariantCharacter = initalInvariantText[column];

                        if (!ClueCharacter.VALIDATION_CHARACTERS_SUPPORTED.Contains(invariantCharacter))
                        {
                            throw new ClueCharacterNotSupportedException(invariantCharacter, initalInvariantText);
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

                        invariantText += invariantCharacter;
                    }
                }

                await _hintService.PostCalculateHintsAsync(puzzle.Id, createdClues);

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

            return (puzzle, design);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to start the puzzle.");

            await DeletePuzzleAsync(db, puzzle.Id);

            throw;
        }
    }
}
