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
        //List<IGrouping<Guid, AnswerCharacter>> answerGroups = await db.Answers
        //    .AsNoTracking()
        //    .Where(a => a.PuzzleId == puzzleId)
        //    .GroupBy(a => a.AnswerId)
        //    .ToListAsync();

        //var answerIdToInvariantTexts = new Dictionary<Guid, string>();
        //foreach (IGrouping<Guid, AnswerCharacter> answerGroup in answerGroups)
        //{
        //    foreach (AnswerCharacter answer in answerGroup)
        //    {
        //        if (answerIdToInvariantTexts.TryGetValue(answer.AnswerId, out string? invariantText))
        //        {
        //            answerIdToInvariantTexts[answer.AnswerId] = $"{invariantText}{answer.InvariantCharacter}";
        //        }
        //        else
        //        {
        //            answerIdToInvariantTexts.Add(answer.AnswerId, $"{answer.InvariantCharacter}");
        //        }
        //    }
        //}

        //return answerIdToInvariantTexts.Values
        //    .Order()
        //    .ToList();
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

    //public async Task AnswerAsync(Guid puzzleId, string invariantText)
    //{
    //    using var db = await _dbContextFactory.CreateDbContextAsync();

    //    Answer? answer = await db.Answers.FirstOrDefaultAsync(a => a.PuzzleId == puzzleId && a.InvariantText == invariantText);
    //    if (answer is not null)
    //    {

    //    }

    //    //bool isFirst = true;
    //    //var currentAnswerIds = new HashSet<Guid>();
    //    //for (int column = 0; column < invariantText.Length; column++)
    //    //{
    //    //    char invariantCharacter = invariantText[column];

    //    //    List<Guid> matchedAnswerIds = await db.AnswerCharacters
    //    //        .Where(a => a.PuzzleId == puzzleId && a.Column == column && a.InvariantCharacter == invariantCharacter && (isFirst || !isFirst && currentAnswerIds.Contains(a.AnswerId)))
    //    //        .Select(a => a.AnswerId)
    //    //        .ToListAsync();

    //    //    if (matchedAnswerIds.Count is <= 0)
    //    //    {
    //    //        return false;
    //    //    }

    //    //    currentAnswerIds = new HashSet<Guid>(matchedAnswerIds);

    //    //    isFirst = false;
    //    //}

    //    //return currentAnswerIds.Count is > 0;
    //}
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
