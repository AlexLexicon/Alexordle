using Alexordle.Client.Application.Database;
using Alexordle.Client.Application.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Alexordle.Client.Application.Services;
public interface IAnswerService
{
    Task<IReadOnlyList<string>> GetAnswerInvariantTextsAsync(Guid puzzleId);
    Task<IReadOnlyList<Answer>> GetAnswersAsync(Guid puzzleId);
    //Task<IReadOnlyList<Answer>> GetAnswersAsync(Guid puzzleId);
    //Task<Answer> CreateAnswerAsync(Guid puzzleId, string answer);
    //Task DeleteAnswerAsync(Guid answerWordId);
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

        List<IGrouping<Guid, Answer>> answerGroups = await db.Answers
            .AsNoTracking()
            .Where(a => a.PuzzleId == puzzleId)
            .GroupBy(a => a.AnswerId)
            .ToListAsync();

        var answerIdToInvariantTexts = new Dictionary<Guid, string>();
        foreach (IGrouping<Guid, Answer> answerGroup in answerGroups)
        {
            foreach (Answer answer in answerGroup)
            {
                if (answerIdToInvariantTexts.TryGetValue(answer.AnswerId, out string? invariantText))
                {
                    answerIdToInvariantTexts[answer.AnswerId] = $"{invariantText}{answer.InvariantCharacter}";
                }
                else
                {
                    answerIdToInvariantTexts.Add(answer.AnswerId, $"{answer.InvariantCharacter}");
                }
            }
        }

        return answerIdToInvariantTexts.Values.ToList();
    }

    public async Task<IReadOnlyList<Answer>> GetAnswersAsync(Guid puzzleId)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        return await db.Answers
            .AsNoTracking()
            .Where(a => a.PuzzleId == puzzleId)
            .ToListAsync();
    }


    //private readonly IDbContextFactory<AlexordleDbContext> _dbContextFactory;
    //private readonly IWordService _wordService;
    //private readonly IGuidProvider _guidProvider;

    //public AnswerService(
    //    IDbContextFactory<AlexordleDbContext> dbContextFactory,
    //    IWordService wordService,
    //    IGuidProvider guidProvider)
    //{
    //    _dbContextFactory = dbContextFactory;
    //    _wordService = wordService;
    //    _guidProvider = guidProvider;
    //}

    //public async Task<IReadOnlyList<Answer>> GetAnswersAsync(Guid puzzleId)
    //{
    //    using var db = await _dbContextFactory.CreateDbContextAsync();

    //    return await db.Answers
    //        .AsNoTracking()
    //        .Where(a => a.PuzzleId == puzzleId)
    //        .ToListAsync();
    //}

    //public async Task<Answer> CreateAnswerAsync(Guid puzzleId, string answer)
    //{
    //    Guid newAnswerWordId = _guidProvider.NewGuid();

    //    string invariantText = await _wordService.WriteWordAsync(newAnswerWordId, answer);

    //    var newAnswer = new Answer
    //    {
    //        WordId = newAnswerWordId,
    //        PuzzleId = puzzleId,
    //        InvariantText = invariantText,
    //    };

    //    await _wordService.CreateWordAsync(newAnswer);

    //    return newAnswer;
    //}

    //public async Task DeleteAnswerAsync(Guid answerWordId)
    //{
    //    try
    //    {
    //        await _wordService.DeleteWordAsync<Answer>(answerWordId);
    //    }
    //    catch
    //    {

    //    }
    //}
}
