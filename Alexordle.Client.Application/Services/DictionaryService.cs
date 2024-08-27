using Alexordle.Client.Application.Database.Entities;

namespace Alexordle.Client.Application.Services;
public interface IDictionaryService
{
    Task<bool> IsSpelledCorrectlyAsync(Guid puzzleId, string invariantText);
}
public class DictionaryService : IDictionaryService
{
    private readonly IWordListsProvider _wordListProvider;
    private readonly IPuzzleService _puzzleService;

    public DictionaryService(
        IWordListsProvider wordListProvider,
        IPuzzleService puzzleService)
    {
        _wordListProvider = wordListProvider;
        _puzzleService = puzzleService;

        WidthToInvariantWords = [];
    }

    private Dictionary<int, HashSet<string>> WidthToInvariantWords { get; }

    public async Task<bool> IsSpelledCorrectlyAsync(Guid puzzleId, string invariantText)
    {
        Puzzle puzzle = await _puzzleService.GetPuzzleAsync(puzzleId);

        bool isSpelledCorrectly = await IsSpelledCorrectlyAsync(puzzle.Width, invariantText);
        if (isSpelledCorrectly)
        {
            return true;
        }

        return false;
    }

    private async Task<bool> IsSpelledCorrectlyAsync(int width, string invariantText)
    {
        if (!WidthToInvariantWords.TryGetValue(width, out HashSet<string>? invariantWords))
        {
            invariantWords = await GetSpecificWordListAsync(width);

            invariantWords = GetInvariantWords(invariantWords);

            WidthToInvariantWords.Add(width, invariantWords);
        }

        return invariantWords.Contains(invariantText);
    }

    private async Task<HashSet<string>> GetSpecificWordListAsync(int width)
    {
        return width switch
        {
            1 => await _wordListProvider.GetWordListForWidth1Async(),
            2 => await _wordListProvider.GetWordListForWidth2Async(),
            3 => await _wordListProvider.GetWordListForWidth3Async(),
            4 => await _wordListProvider.GetWordListForWidth4Async(),
            5 => await _wordListProvider.GetWordListForWidth5Async(),
            6 => await _wordListProvider.GetWordListForWidth6Async(),
            7 => await _wordListProvider.GetWordListForWidth7Async(),
            8 => await _wordListProvider.GetWordListForWidth8Async(),
            9 => await _wordListProvider.GetWordListForWidth9Async(),
            10 => await _wordListProvider.GetWordListForWidth10Async(),
            _ => throw new NotSupportedException($"The width '{width}' is not supported."),
        };
    }

    private HashSet<string> GetInvariantWords(IEnumerable<string> wordList)
    {
        var invariantWords = new HashSet<string>();
        foreach (string word in wordList)
        {
            invariantWords.Add(word.ToUpperInvariant());
        }

        return invariantWords;
    }
}
