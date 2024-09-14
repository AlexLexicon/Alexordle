using Alexordle.Client.Application.Database;
using Alexordle.Client.Application.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Alexordle.Client.Application.Services;
public interface IClueService
{
    Task<IReadOnlyList<string>> GetClueInvariantTextsAsync(Guid puzzleId);
    Task<IReadOnlyList<ClueCharacter>> GetCluesAsync(Guid puzzleId, int row);
}
public class ClueService : IClueService
{
    private readonly IDbContextFactory<AlexordleDbContext> _dbContextFactory;

    public ClueService(IDbContextFactory<AlexordleDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IReadOnlyList<string>> GetClueInvariantTextsAsync(Guid puzzleId)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        return await db.Clues
             .Where(a => a.PuzzleId == puzzleId)
             .OrderBy(a => a.Order)
             .Select(a => a.InvariantText)
             .ToListAsync();
    }

    public async Task<IReadOnlyList<ClueCharacter>> GetCluesAsync(Guid puzzleId, int row)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        return await db.ClueCharacters
            .AsNoTracking()
            .Where(c => c.PuzzleId == puzzleId && c.Row == row)
            .OrderBy(c => c.Column)
            .ToListAsync();
    }
}
