using Alexordle.Client.Application.Database;
using Alexordle.Client.Application.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Alexordle.Client.Application.Services;
public interface IAnswerService
{
    Task<IReadOnlyList<string>> GetAnswerInvariantTextsAsync(Guid puzzleId);
    Task<IReadOnlyList<AnswerCharacter>> GetAnswersAsync(Guid puzzleId);
    Task<bool> IsAnsweredAsync(Guid puzzleId, char invariantCharacter);
}
public class AnswerService : IAnswerService
{
    private readonly IDbContextFactory<AlexordleDbContext> _dbContextFactory;

    public AnswerService(IDbContextFactory<AlexordleDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IReadOnlyList<string>> GetAnswerInvariantTextsAsync(Guid puzzleId)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        return await db.Answers
            .Where(a => a.PuzzleId == puzzleId)
            .OrderBy(a => a.InvariantText)
            .Select(a => a.InvariantText)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<AnswerCharacter>> GetAnswersAsync(Guid puzzleId)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        return await db.AnswerCharacters
            .AsNoTracking()
            .Where(a => a.PuzzleId == puzzleId)
            .OrderBy(a => a.Column)
            .ToListAsync();
    }

    public async Task<bool> IsAnsweredAsync(Guid puzzleId, char invariantCharacter)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        return await db.AnswerCharacters
            .Where(ac => ac.PuzzleId == puzzleId && ac.InvariantCharacter == invariantCharacter)
            .Join(db.Answers, ac => ac.AnswerId, a => a.Id, (ac, a) => a)
            .Where(a => a.IsSolved)
            .AnyAsync();
    }
}
