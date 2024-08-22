using Alexordle.Client.Application.Database.Entities;

namespace Alexordle.Client.Application.Services;
public interface IDictionaryService
{
    Task<bool> CheckGuessSpellingAsync(Guid guessWordId);
    Task<bool> CheckSpellingAsync(string invariantWord, Guid puzzleId);
}
public class DictionaryService : IDictionaryService
{
    private readonly IWordListProvider _wordListProvider;
    private readonly IGuessService _guessService;
    private readonly ILetterService _letterService;
    private readonly IPuzzleService _puzzleService;
    private readonly IAnswerService _answerService;

    public DictionaryService(
        IWordListProvider wordListProvider,
        IGuessService guessService,
        ILetterService letterService,
        IPuzzleService puzzleService,
        IAnswerService answerService)
    {
        _wordListProvider = wordListProvider;
        _guessService = guessService;
        _letterService = letterService;
        _puzzleService = puzzleService;
        _answerService = answerService;

        WidthToInvariantWords = [];
        PuzzleIdToAnswerInvariantWords = [];
    }

    private Dictionary<int, HashSet<string>> WidthToInvariantWords { get; }
    private Dictionary<Guid, HashSet<string>> PuzzleIdToAnswerInvariantWords { get; }

    public async Task<bool> CheckGuessSpellingAsync(Guid guessWordId)
    {
        Guess guess = await _guessService.GetGuessAsync(guessWordId);

        string text;
        if (guess.IsCommitted && guess.CommittedInvariantText is not null)
        {
            text = guess.CommittedInvariantText;
        }
        else
        {
            text = await _letterService.GetStringAsync(guess.WordId);
        }

        return await CheckSpellingAsync(text, guess.PuzzleId);
    }

    public async Task<bool> CheckSpellingAsync(string invariantWord, Guid puzzleId)
    {
        Puzzle puzzle = await _puzzleService.GetPuzzleAsync(puzzleId);

        HashSet<string> words = await GetWordListAsync(puzzle.Width);
        if (words.Contains(invariantWord))
        {
            return true;
        }

        HashSet<string> answers = await GetAnswersAsync(puzzle.Id);
        if (answers.Contains(invariantWord))
        {
            return true;
        }

        return false;
    }

    private async Task<HashSet<string>> GetWordListAsync(int width)
    {
        if (!WidthToInvariantWords.TryGetValue(width, out HashSet<string>? words))
        {
            words = await LoadWordListAsync(width);

            words = GetInvariantWords(words);

            WidthToInvariantWords.Add(width, words);
        }

        return words;
    }

    private async Task<HashSet<string>> GetAnswersAsync(Guid puzzleId)
    {
        if (!PuzzleIdToAnswerInvariantWords.TryGetValue(puzzleId, out HashSet<string>? answerWords))
        {
            answerWords = [];

            IReadOnlyList<Answer> answers = await _answerService.GetAnswersAsync(puzzleId);
            foreach (Answer answer in answers)
            {
                answerWords.Add(answer.InvariantText);
            }

            PuzzleIdToAnswerInvariantWords.Add(puzzleId, answerWords);
        }

        return answerWords;
    }

    private async Task<HashSet<string>> LoadWordListAsync(int width)
    {
        return width switch
        {
            5 => await _wordListProvider.LoadWords5Async(),
            _ => throw new NotSupportedException($"The width '{width}' is not supported."),
        };
    }

    private HashSet<string> GetInvariantWords(IEnumerable<string> words)
    {
        var invariantWords = new HashSet<string>();
        foreach (string word in words)
        {
            string invariantWord = LetterService.MakeInvariant(word);

            invariantWords.Add(invariantWord);
        }

        return invariantWords;
    }
}
