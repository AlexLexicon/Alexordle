using Alexordle.Client.Application.Database;
using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Exceptions;
using Lexicom.DependencyInjection.Primitives;
using Microsoft.EntityFrameworkCore;

namespace Alexordle.Client.Application.Services;
public interface IPuzzleService
{
    Task<Puzzle> GetPuzzleAsync(Guid puzzleId);
    Task<Puzzle> CreatePuzzleAsync();
    Task SetWidthAsync(Guid puzzleId, int width);
    Task SetMaximumGuessesAsync(Guid puzzleId, int maximumGuesses);
    Task SetIsSpellCheckingAsync(Guid puzzleId, bool isSpellChecking);
    Task DeleteAllPuzzlesAsync();
    Task DeletePuzzleAsync(Guid puzzleId);
}
public class PuzzleService : IPuzzleService
{
    private readonly IDbContextFactory<AlexordleDbContext> _dbContextFactory;
    private readonly IGuidProvider _guidProvider;
    private readonly IClueService _clueService;
    private readonly IAnswerService _answerService;

    public PuzzleService(
        IDbContextFactory<AlexordleDbContext> dbContextFactory,
        IGuidProvider guidProvider,
        IClueService clueService,
        IAnswerService answerService)
    {
        _dbContextFactory = dbContextFactory;
        _guidProvider = guidProvider;
        _clueService = clueService;
        _answerService = answerService;
    }

    public async Task<Puzzle> GetPuzzleAsync(Guid puzzleId)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        return await GetPuzzleAsync(puzzleId, db);  
    }

    public async Task<Puzzle> CreatePuzzleAsync()
    {
        var puzzle = new Puzzle
        {
            Id = _guidProvider.NewGuid(),
            MaximumGuesses = 0,
            Width = 0,
            IsSpellChecking = false,
        };

        using var db = await _dbContextFactory.CreateDbContextAsync();

        await db.Puzzles.AddAsync(puzzle);

        await db.SaveChangesAsync();

        return puzzle;
    }

    public async Task SetWidthAsync(Guid puzzleId, int width)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        Puzzle puzzle = await GetPuzzleAsync(puzzleId, db);

        if (puzzle.Width == width)
        {
            return;
        }

        puzzle.Width = width;

        db.Update(puzzle);

        await db.SaveChangesAsync();
    }

    public async Task SetMaximumGuessesAsync(Guid puzzleId, int maximumGuesses)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        Puzzle puzzle = await GetPuzzleAsync(puzzleId, db);

        if (puzzle.MaximumGuesses == maximumGuesses)
        {
            return;
        }

        puzzle.MaximumGuesses = maximumGuesses;

        db.Update(puzzle);  

        await db.SaveChangesAsync();
    }

    public async Task SetIsSpellCheckingAsync(Guid puzzleId, bool isSpellChecking)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        Puzzle puzzle = await GetPuzzleAsync(puzzleId, db);

        if (puzzle.IsSpellChecking == isSpellChecking)
        {
            return;
        }

        puzzle.IsSpellChecking = isSpellChecking;

        db.Update(puzzle);

        await db.SaveChangesAsync();
    }

    public async Task DeleteAllPuzzlesAsync()
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        List<Puzzle> puzzles = await db.Puzzles
            .AsNoTracking()
            .ToListAsync();

        foreach (Puzzle puzzle in puzzles)
        {
            await DeletePuzzleAsync(puzzle, db);
        }
    }

    public async Task DeletePuzzleAsync(Guid puzzleId)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        Puzzle puzzle = await GetPuzzleAsync(puzzleId, db);

        await DeletePuzzleAsync(puzzle, db);
    }

    private async Task DeletePuzzleAsync(Puzzle puzzle, AlexordleDbContext db)
    {
        db.Puzzles.Remove(puzzle);

        IReadOnlyList<Clue> clues = await _clueService.GetCluesAsync(puzzle.Id);
        foreach (Clue clue in clues)
        {
            await _clueService.DeleteClueAsync(clue.WordId);
        }

        IReadOnlyList<Answer> answers = await _answerService.GetAnswersAsync(puzzle.Id);
        foreach (Answer answer in answers)
        {
            await _answerService.DeleteAnswerAsync(answer.WordId);
        }

        await db.SaveChangesAsync();
    }

    private async Task<Puzzle> GetPuzzleAsync(Guid puzzleId, AlexordleDbContext db)
    {
        Puzzle? puzzle = await db.Puzzles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == puzzleId);

        if (puzzle is null)
        {
            throw new PuzzleDoesNotExistException(puzzleId);
        }

        return puzzle;
    }
}
