using Alexordle.Client.Application.Database;
using Alexordle.Client.Application.Database.Entities;
using Lexicom.DependencyInjection.Primitives;
using Microsoft.EntityFrameworkCore;

namespace Alexordle.Client.Application.Services;
public interface IClueService
{
    Task<IReadOnlyList<Clue>> GetCluesAsync(Guid puzzleId);
    Task<Clue> CreateClueAsync(Guid puzzleId, string clue);
    Task DeleteClueAsync(Guid clueWordId);
}
public class ClueService : IClueService
{
    private readonly IDbContextFactory<AlexordleDbContext> _dbContextFactory;
    private readonly IWordService _wordService;
    private readonly IGuidProvider _guidProvider;
    private readonly ITimestampService _timestampService;

    public ClueService(
        IDbContextFactory<AlexordleDbContext> dbContextFactory,
        IWordService wordService,
        IGuidProvider guidProvider,
        ITimestampService timestampService)
    {
        _dbContextFactory = dbContextFactory;
        _wordService = wordService;
        _guidProvider = guidProvider;
        _timestampService = timestampService;
    }

    public async Task<IReadOnlyList<Clue>> GetCluesAsync(Guid puzzleId)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        return await db.Clues
            .AsNoTracking()
            .Where(c => c.PuzzleId == puzzleId)
            .OrderBy(c => c.CreatedTimestamp)
            .ToListAsync();
    }

    public async Task<Clue> CreateClueAsync(Guid puzzleId, string clue)
    {
        long timestamp = await _timestampService.GetTimestampAsync();

        Guid newClueWordId = _guidProvider.NewGuid();

        string invariantText = await _wordService.WriteWordAsync(newClueWordId, clue);

        var newClue = new Clue
        {
            WordId = newClueWordId,
            PuzzleId = puzzleId,
            CreatedTimestamp = timestamp,
            InvariantText = invariantText,
        };

        await _wordService.CreateWordAsync(newClue);

        return newClue;
    }

    public async Task DeleteClueAsync(Guid clueWordId)
    {
        await _wordService.DeleteWordAsync<Clue>(clueWordId);
    }
}
