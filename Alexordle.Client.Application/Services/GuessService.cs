using Alexordle.Client.Application.Database;
using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Alexordle.Client.Application.Services;
public interface IGuessService
{
    /// <exception cref="GuessDoesNotExistException"></exception>
    Task<Guess> GetGuessAsync(Guid guessId);
    Task<IReadOnlyList<string>> GetGuessInvariantTextsAsync(Guid puzzleId);
    Task<IReadOnlyList<GuessCharacter>> GetGuessesAsync(Guid puzzleId, int row);
}
public class GuessService : IGuessService
{
    private readonly IDbContextFactory<AlexordleDbContext> _dbContextFactory;

    public GuessService(IDbContextFactory<AlexordleDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Guess> GetGuessAsync(Guid guessId)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        Guess? guess = await db.Guesses.FirstOrDefaultAsync(g => g.Id == guessId);
        if (guess is null)
        {
            throw new GuessDoesNotExistException(guessId);
        }

        return guess;
    }

    public async Task<IReadOnlyList<string>> GetGuessInvariantTextsAsync(Guid puzzleId)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        return await db.Guesses
             .Where(a => a.PuzzleId == puzzleId)
             .OrderBy(a => a.InvariantText)
             .Select(a => a.InvariantText)
             .ToListAsync();
    }

    public async Task<IReadOnlyList<GuessCharacter>> GetGuessesAsync(Guid puzzleId, int row)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        return await db.GuessCharacters
            .AsNoTracking()
            .Where(g => g.PuzzleId == puzzleId && g.Row == row)
            .OrderBy(g => g.Column)
            .ToListAsync();
    }
}
