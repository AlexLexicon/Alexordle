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

        if (answersList.Count is < Answer.VALIDATION_COUNT_MINIMUM)
        {
            throw new ArgumentLessThanException(Answer.VALIDATION_COUNT_MINIMUM, answersList.Count);
        }

        if (answersList.Count is > Answer.VALIDATION_COUNT_MAXIMUM)
        {
            throw new ArgumentGreaterThanException(Answer.VALIDATION_COUNT_MAXIMUM, answersList.Count);
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
            IsComplete = false,
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

                    if (!Answer.VALIDATION_CHARACTERS_SUPPORTED.Contains(invariantCharacter))
                    {
                        throw new AnswerCharacterNotSupportedException(invariantCharacter, invariantText);
                    }

                    bool anotherAnswerContainsCharacter = await db.Answers
                        .AsNoTracking()
                        .AnyAsync(ac => ac.AnswerId != answerId && ac.InvariantCharacter == invariantCharacter);

                    if (anotherAnswerContainsCharacter)
                    {
                        throw new DuplicateAnswerCharacterException(invariantCharacter);
                    }

                    await db.Answers.AddAsync(new Answer
                    {
                        PuzzleId = puzzle.Id,
                        AnswerId = answerId,
                        Column = column,
                        InvariantCharacter = invariantCharacter,
                    });

                    await db.SaveChangesAsync();
                }
            }

            for (int row = 0; row < cluesList.Count; row++)
            {
                string invariantText = cluesList[row].ToUpperInvariant();

                if (invariantText.Length != puzzle.Width)
                {
                    throw new WidthMismatchException(invariantText, invariantText.Length, puzzle.Width);
                }

                var createdClues = new List<Clue>();
                for (int column = 0; column < invariantText.Length; column++)
                {
                    char invariantCharacter = invariantText[column];

                    if (!Clue.VALIDATION_CHARACTERS_SUPPORTED.Contains(invariantCharacter))
                    {
                        throw new ClueCharacterNotSupportedException(invariantCharacter, invariantText);
                    }

                    var clue = new Clue
                    {
                        PuzzleId = puzzle.Id,
                        Row = row,
                        Column = column,
                        InvariantCharacter = invariantCharacter,
                        Hint = Hints.None,
                    };

                    await _hintService.CalculateHintAsync(clue);

                    await db.Clues.AddAsync(clue);

                    await db.SaveChangesAsync();

                    createdClues.Add(clue);
                }

                await _hintService.PostCalculateHintsAsync(createdClues);
            }

            await db.Puzzles.AddAsync(puzzle);

            await db.SaveChangesAsync();

            return puzzle;
        }
        catch
        {
            List<Answer> answersToRemove = await db.Answers
                .AsNoTracking()
                .Where(a => a.PuzzleId == puzzle.Id)
                .ToListAsync();
            db.Answers.RemoveRange(answersToRemove);

            List<Clue> cluesToRemove = await db.Clues
                .AsNoTracking()
                .Where(c => c.PuzzleId == puzzle.Id)
                .ToListAsync();
            db.Clues.RemoveRange(cluesToRemove);

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
    //private readonly IDbContextFactory<AlexordleDbContext> _dbContextFactory;
    //private readonly IGuidProvider _guidProvider;
    //private readonly IClueService _clueService;
    //private readonly IAnswerService _answerService;

    //public PuzzleService(
    //    IDbContextFactory<AlexordleDbContext> dbContextFactory,
    //    IGuidProvider guidProvider,
    //    IClueService clueService,
    //    IAnswerService answerService)
    //{
    //    _dbContextFactory = dbContextFactory;
    //    _guidProvider = guidProvider;
    //    _clueService = clueService;
    //    _answerService = answerService;
    //}

    //public async Task<Puzzle> GetPuzzleAsync(Guid puzzleId)
    //{
    //    using var db = await _dbContextFactory.CreateDbContextAsync();

    //    return await GetPuzzleAsync(puzzleId, db);  
    //}

    //public async Task<Puzzle> CreatePuzzleAsync()
    //{
    //    var puzzle = new Puzzle
    //    {
    //        Id = _guidProvider.NewGuid(),
    //        MaximumGuesses = 0,
    //        Width = 0,
    //        IsSpellChecking = false,
    //    };

    //    using var db = await _dbContextFactory.CreateDbContextAsync();

    //    await db.Puzzles.AddAsync(puzzle);

    //    await db.SaveChangesAsync();

    //    return puzzle;
    //}

    //public async Task SetWidthAsync(Guid puzzleId, int width)
    //{
    //    using var db = await _dbContextFactory.CreateDbContextAsync();

    //    Puzzle puzzle = await GetPuzzleAsync(puzzleId, db);

    //    if (puzzle.Width == width)
    //    {
    //        return;
    //    }

    //    puzzle.Width = width;

    //    db.Update(puzzle);

    //    await db.SaveChangesAsync();
    //}

    //public async Task SetMaximumGuessesAsync(Guid puzzleId, int maximumGuesses)
    //{
    //    using var db = await _dbContextFactory.CreateDbContextAsync();

    //    Puzzle puzzle = await GetPuzzleAsync(puzzleId, db);

    //    if (puzzle.MaximumGuesses == maximumGuesses)
    //    {
    //        return;
    //    }

    //    puzzle.MaximumGuesses = maximumGuesses;

    //    db.Update(puzzle);  

    //    await db.SaveChangesAsync();
    //}

    //public async Task SetIsSpellCheckingAsync(Guid puzzleId, bool isSpellChecking)
    //{
    //    using var db = await _dbContextFactory.CreateDbContextAsync();

    //    Puzzle puzzle = await GetPuzzleAsync(puzzleId, db);

    //    if (puzzle.IsSpellChecking == isSpellChecking)
    //    {
    //        return;
    //    }

    //    puzzle.IsSpellChecking = isSpellChecking;

    //    db.Update(puzzle);

    //    await db.SaveChangesAsync();
    //}

    //public async Task DeleteAllPuzzlesAsync()
    //{
    //    using var db = await _dbContextFactory.CreateDbContextAsync();

    //    List<Puzzle> puzzles = await db.Puzzles
    //        .AsNoTracking()
    //        .ToListAsync();

    //    foreach (Puzzle puzzle in puzzles)
    //    {
    //        await DeletePuzzleAsync(puzzle, db);
    //    }
    //}

    //public async Task DeletePuzzleAsync(Guid puzzleId)
    //{
    //    using var db = await _dbContextFactory.CreateDbContextAsync();

    //    Puzzle puzzle = await GetPuzzleAsync(puzzleId, db);

    //    await DeletePuzzleAsync(puzzle, db);
    //}

    //private async Task DeletePuzzleAsync(Puzzle puzzle, AlexordleDbContext db)
    //{
    //    db.Puzzles.Remove(puzzle);

    //    IReadOnlyList<Clue> clues = await _clueService.GetCluesAsync(puzzle.Id);
    //    foreach (Clue clue in clues)
    //    {
    //        await _clueService.DeleteClueAsync(clue.WordId);
    //    }

    //    IReadOnlyList<Answer> answers = await _answerService.GetAnswersAsync(puzzle.Id);
    //    foreach (Answer answer in answers)
    //    {
    //        await _answerService.DeleteAnswerAsync(answer.WordId);
    //    }

    //    await db.SaveChangesAsync();
    //}

    //private async Task<Puzzle> GetPuzzleAsync(Guid puzzleId, AlexordleDbContext db)
    //{
    //    Puzzle? puzzle = await db.Puzzles
    //        .AsNoTracking()
    //        .FirstOrDefaultAsync(p => p.Id == puzzleId);

    //    if (puzzle is null)
    //    {
    //        throw new PuzzleDoesNotExistException(puzzleId);
    //    }

    //    return puzzle;
    //}
}
