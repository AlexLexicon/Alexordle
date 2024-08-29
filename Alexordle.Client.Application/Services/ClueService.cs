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
             .OrderBy(a => a.InvariantText)
             .Select(a => a.InvariantText)
             .ToListAsync();
        //List<IGrouping<int, ClueCharacter>> clueGroups = await db.ClueCharacters
        //    .AsNoTracking()
        //    .Where(c => c.PuzzleId == puzzleId)
        //    .GroupBy(c => c.Row)
        //    .ToListAsync();

        //var clueRowToInvariantTexts = new Dictionary<int, string>();
        //foreach (IGrouping<int, ClueCharacter> clueGroup in clueGroups)
        //{
        //    foreach (ClueCharacter clue in clueGroup)
        //    {
        //        if (clueRowToInvariantTexts.TryGetValue(clue.Row, out string? invariantText))
        //        {
        //            clueRowToInvariantTexts[clue.Row] = $"{invariantText}{clue.InvariantCharacter}";
        //        }
        //        else
        //        {
        //            clueRowToInvariantTexts.Add(clue.Row, $"{clue.InvariantCharacter}");
        //        }
        //    }
        //}

        //return clueRowToInvariantTexts.Values
        //    .Order()
        //    .ToList();
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
    //private readonly IDbContextFactory<AlexordleDbContext> _dbContextFactory;
    //private readonly IWordService _wordService;
    //private readonly IGuidProvider _guidProvider;
    //private readonly ITimestampService _timestampService;

    //public ClueService(
    //    IDbContextFactory<AlexordleDbContext> dbContextFactory,
    //    IWordService wordService,
    //    IGuidProvider guidProvider,
    //    ITimestampService timestampService)
    //{
    //    _dbContextFactory = dbContextFactory;
    //    _wordService = wordService;
    //    _guidProvider = guidProvider;
    //    _timestampService = timestampService;
    //}

    //public async Task<IReadOnlyList<Clue>> GetCluesAsync(Guid puzzleId)
    //{
    //    using var db = await _dbContextFactory.CreateDbContextAsync();

    //    return await db.Clues
    //        .AsNoTracking()
    //        .Where(c => c.PuzzleId == puzzleId)
    //        .OrderBy(c => c.CreatedTimestamp)
    //        .ToListAsync();
    //}

    //public async Task<Clue> CreateClueAsync(Guid puzzleId, string clue)
    //{
    //    long timestamp = await _timestampService.GetTimestampAsync();

    //    Guid newClueWordId = _guidProvider.NewGuid();

    //    string invariantText = await _wordService.WriteWordAsync(newClueWordId, clue);

    //    var newClue = new Clue
    //    {
    //        WordId = newClueWordId,
    //        PuzzleId = puzzleId,
    //        CreatedTimestamp = timestamp,
    //        InvariantText = invariantText,
    //    };

    //    await _wordService.CreateWordAsync(newClue);

    //    return newClue;
    //}

    //public async Task DeleteClueAsync(Guid clueWordId)
    //{
    //    await _wordService.DeleteWordAsync<Clue>(clueWordId);
    //}
}
