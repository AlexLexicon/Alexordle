using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Models;

namespace Alexordle.Client.Application.Services;
public interface IStateService
{
    Task<State> GetStateAsync(Guid puzzleId);
}
public class StateService : IStateService
{
    private readonly IAnswerService _answerService;
    private readonly IGuessService _guessService;
    private readonly IPuzzleService _puzzleService;

    public StateService(
        IAnswerService answerService,
        IGuessService guessService,
        IPuzzleService puzzleService)
    {
        _answerService = answerService;
        _guessService = guessService;
        _puzzleService = puzzleService;
    }

    public async Task<State> GetStateAsync(Guid puzzleId)
    {
        var getPuzzleTask = _puzzleService.GetPuzzleAsync(puzzleId);
        var getAnswersTask = _answerService.GetAnswersAsync(puzzleId);
        var getCommittedGuessesTask = _guessService.GetCommittedGuessesAsync(puzzleId);

        Puzzle puzzle = await getPuzzleTask;
        IReadOnlyList<Answer> answers = await getAnswersTask;
        IReadOnlyList<Guess> guesses = await getCommittedGuessesTask;

        int answersCount = answers.Count;
        var correctGuessIds = new HashSet<Guid>();
        bool isLastGuessCorrect = false;
        foreach (var guess in guesses)
        {
            bool isCorrect = await _guessService.IsCommittedGuessCorrectAsync(guess.WordId);
            if (isCorrect)
            {
                correctGuessIds.Add(guess.WordId);
            }

            isLastGuessCorrect = isCorrect;
        }

        Guess? latestGuess = guesses
            .OrderByDescending(g => g.CommittedTimestamp)
            .FirstOrDefault();

        int remainingAnswers = answersCount - correctGuessIds.Count;
        int remainingGuesses = puzzle.MaximumGuesses - guesses.Count;
        bool isBonus = remainingGuesses is <= 0 && latestGuess is not null && isLastGuessCorrect;
        bool isVictory = remainingAnswers is 0;
        bool isDefeat = remainingGuesses is 0 && !isVictory && !isBonus;

        return new State
        {
            TotalAnswers = answersCount,
            TotalGuesses = guesses.Count,
            MaximumGuesses = puzzle.MaximumGuesses,
            RemainingAnswers = remainingAnswers,
            RemainingGuesses = remainingGuesses,
            IsBonus = isBonus,
            IsVictory = isVictory,
            IsDefeat = isDefeat,
            CorrectGuessIds = correctGuessIds,
        };
    }
}
