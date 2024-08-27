using Alexordle.Client.Application.Database;
using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Alexordle.Client.Application.Services;
public interface IGuessService
{
    Task<IReadOnlyList<string>> GetGuessInvariantTextsAsync(Guid puzzleId);
    Task<IReadOnlyList<Guess>> GetGuessesAsync(Guid puzzleId, int row);
    //Task<IReadOnlyList<Guess>> GetCommittedGuessesAsync(Guid puzzleId);
    //Task<IReadOnlyList<Guess>> GetGuessesAsync(Guid puzzleId);
    //Task<Guess> GetGuessAsync(Guid guessWordId);
    //Task<Guess> CreateGuessAsync(Guid puzzleId);
    //Task<bool> IsCommittedGuessCorrectAsync(Guid guessWordId);
    //Task<bool> IsAlreadyGuessedAsync(Guid guessWordId);
    //Task<bool> IsGuessCompleteAsync(Guid guessWordId);
    //Task SubmitGuessAsync(Guid guessWordId);
    //Task DeleteGuessAsync(Guid guessWordId);
}
public class GuessService : IGuessService
{
    private readonly IDbContextFactory<AlexordleDbContext> _dbContextFactory;

    public GuessService(IDbContextFactory<AlexordleDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IReadOnlyList<string>> GetGuessInvariantTextsAsync(Guid puzzleId)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        List<IGrouping<int, Guess>> guessGroups = await db.Guesses
            .AsNoTracking()
            .Where(a => a.PuzzleId == puzzleId)
            .GroupBy(a => a.Row)
            .ToListAsync();

        var guessRowToInvariantTexts = new Dictionary<int, string>();
        foreach (IGrouping<int, Guess> guessGroup in guessGroups)
        {
            foreach (Guess guess in guessGroup)
            {
                if (guessRowToInvariantTexts.TryGetValue(guess.Row, out string? invariantText))
                {
                    guessRowToInvariantTexts[guess.Row] = $"{invariantText}{guess.InvariantCharacter}";
                }
                else
                {
                    guessRowToInvariantTexts.Add(guess.Row, $"{guess.InvariantCharacter}");
                }
            }
        }

        return guessRowToInvariantTexts.Values.ToList();
    }

    public async Task<IReadOnlyList<Guess>> GetGuessesAsync(Guid puzzleId, int row)
    {
        using var db = await _dbContextFactory.CreateDbContextAsync();

        return await db.Guesses
            .AsNoTracking()
            .Where(c => c.PuzzleId == puzzleId && c.Row == row)
            .ToListAsync();
    }
    //private readonly IDbContextFactory<AlexordleDbContext> _dbContextFactory;
    //private readonly IGuidProvider _guidProvider;
    //private readonly ITimestampService _timestampService;
    //private readonly IWordService _wordService;
    //private readonly ILetterService _letterService;
    //private readonly IAnswerService _answerService;
    //private readonly IPuzzleService _puzzleService;

    //public GuessService(
    //    IDbContextFactory<AlexordleDbContext> dbContextFactory,
    //    IGuidProvider guidProvider,
    //    ITimestampService timestampService,
    //    IWordService wordService,
    //    ILetterService letterService,
    //    IAnswerService answerService,
    //    IPuzzleService puzzleService)
    //{
    //    _dbContextFactory = dbContextFactory;
    //    _guidProvider = guidProvider;
    //    _timestampService = timestampService;
    //    _wordService = wordService;
    //    _letterService = letterService;
    //    _answerService = answerService;
    //    _puzzleService = puzzleService;
    //}

    //public async Task<IReadOnlyList<Guess>> GetCommittedGuessesAsync(Guid puzzleId)
    //{
    //    using var db = await _dbContextFactory.CreateDbContextAsync();

    //    return await GetCommittedGuessesAsync(puzzleId, db);
    //}

    //public async Task<IReadOnlyList<Guess>> GetGuessesAsync(Guid puzzleId)
    //{
    //    using var db = await _dbContextFactory.CreateDbContextAsync();

    //    var commitedGuesses = await GetCommittedGuessesAsync(puzzleId, db);

    //    var uncommittedGuesses = await db.Guesses
    //        .AsNoTracking()
    //        .Where(g => g.PuzzleId == puzzleId && !g.IsCommitted)
    //        .OrderBy(g => g.CreatedTimestamp)
    //        .ToListAsync();

    //    var orderedGuesses = new List<Guess>(commitedGuesses);
    //    orderedGuesses.AddRange(uncommittedGuesses);

    //    return orderedGuesses;
    //}

    //public async Task<Guess> GetGuessAsync(Guid guessWordId)
    //{
    //    return await _wordService.GetWordAsync<Guess>(guessWordId);
    //}

    //public async Task<Guess> CreateGuessAsync(Guid puzzleId)
    //{
    //    long timestamp = await _timestampService.GetTimestampAsync();

    //    var guess = new Guess
    //    {
    //        WordId = _guidProvider.NewGuid(),
    //        PuzzleId = puzzleId,
    //        CreatedTimestamp = timestamp,
    //        CommittedTimestamp = 0,
    //        IsCommitted = false,
    //        CommittedInvariantText = null,
    //    };

    //    await _wordService.CreateWordAsync(guess);

    //    return guess;
    //}

    //public async Task SubmitGuessAsync(Guid guessWordId)
    //{
    //    Guess guess = await _wordService.GetWordAsync<Guess>(guessWordId);

    //    if (guess.IsCommitted)
    //    {
    //        return;
    //    }

    //    guess.CommittedInvariantText = await _letterService.GetStringAsync(guessWordId);
    //    guess.IsCommitted = true;

    //    using var db = await _dbContextFactory.CreateDbContextAsync();

    //    db.Guesses.Update(guess);

    //    await db.SaveChangesAsync();
    //}

    //public async Task<bool> IsCommittedGuessCorrectAsync(Guid guessWordId)
    //{
    //    Guess guess = await _wordService.GetWordAsync<Guess>(guessWordId);

    //    if (!guess.IsCommitted || guess.CommittedInvariantText is null)
    //    {
    //        return false;
    //    }

    //    IReadOnlyList<Answer> answers = await _answerService.GetAnswersAsync(guess.PuzzleId);
    //    foreach (Answer answer in answers)
    //    {
    //        if (guess.CommittedInvariantText == answer.InvariantText)
    //        {
    //            return true;
    //        }
    //    }

    //    return false;
    //}

    //public async Task<bool> IsAlreadyGuessedAsync(Guid guessWordId)
    //{
    //    Guess guess = await GetGuessAsync(guessWordId);

    //    IReadOnlyList<Guess> committedGuesses = await GetCommittedGuessesAsync(guess.PuzzleId);

    //    string invariantText = await _letterService.GetStringAsync(guessWordId);
    //    foreach (var committedGuess in committedGuesses)
    //    {
    //        if (committedGuess.CommittedInvariantText is not null && committedGuess.CommittedInvariantText == invariantText)
    //        {
    //            return true;
    //        }
    //    }

    //    return false;
    //}

    //public async Task<bool> IsGuessCompleteAsync(Guid guessWordId)
    //{
    //    Guess guess = await GetGuessAsync(guessWordId);

    //    var getPuzzleTask = _puzzleService.GetPuzzleAsync(guess.PuzzleId);
    //    var getStringTask = _letterService.GetStringAsync(guessWordId);

    //    Puzzle puzzle = await getPuzzleTask;
    //    string invariantText = await getStringTask;

    //    return invariantText.Length == puzzle.Width;
    //}

    //public async Task DeleteGuessAsync(Guid guessWordId)
    //{
    //    await _wordService.DeleteWordAsync<Guess>(guessWordId);
    //}

    //private async Task<IReadOnlyList<Guess>> GetCommittedGuessesAsync(Guid puzzleId, AlexordleDbContext db)
    //{
    //    return await db.Guesses
    //        .AsNoTracking()
    //        .Where(g => g.PuzzleId == puzzleId && g.IsCommitted)
    //        .OrderBy(g => g.CommittedTimestamp)
    //        .ToListAsync();
    //}
}
