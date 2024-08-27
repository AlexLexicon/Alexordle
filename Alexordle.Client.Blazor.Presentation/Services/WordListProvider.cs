using Alexordle.Client.Application.Services;
using Lexicom.Http.Extensions;

namespace Alexordle.Client.Blazor.Presentation.Services;
public class WordListProvider : IWordListsProvider
{
    private readonly IHttpClientFactory _httpClientFactory;

    public WordListProvider(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public Task<HashSet<string>> GetWordListForWidth1Async()
    {
        var wordList = new HashSet<string>
        {
            "a",
            "b",
            "c",
            "d",
            "e",
            "f",
            "g",
            "h",
            "i",
            "j",
            "k",
            "l",
            "m",
            "n",
            "o",
            "p",
            "q",
            "r",
            "s",
            "t",
            "u",
            "v",
            "w",
            "x",
            "y",
            "z",
        };

        return Task.FromResult(wordList);
    }

    public async Task<HashSet<string>> GetWordListForWidth2Async()
    {
        return await DownloadWordListAsync("wordlist2");
    }

    public async Task<HashSet<string>> GetWordListForWidth3Async()
    {
        return await DownloadWordListAsync("wordlist3");
    }

    public async Task<HashSet<string>> GetWordListForWidth4Async()
    {
        return await DownloadWordListAsync("wordlist4");
    }

    public async Task<HashSet<string>> GetWordListForWidth5Async()
    {
        return await DownloadWordListAsync("wordlist5");
    }

    public async Task<HashSet<string>> GetWordListForWidth6Async()
    {
        return await DownloadWordListAsync("wordlist6");
    }

    public async Task<HashSet<string>> GetWordListForWidth7Async()
    {
        return await DownloadWordListAsync("wordlist7");
    }

    public async Task<HashSet<string>> GetWordListForWidth8Async()
    {
        return await DownloadWordListAsync("wordlist8");
    }

    public async Task<HashSet<string>> GetWordListForWidth9Async()
    {
        return await DownloadWordListAsync("wordlist9");
    }

    public async Task<HashSet<string>> GetWordListForWidth10Async()
    {
        return await DownloadWordListAsync("wordlist10");
    }

    private async Task<HashSet<string>> DownloadWordListAsync(string jsonFileName)
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("wwwroot");

        return await httpClient.GetFromJsonNotNullAsync<HashSet<string>>($"{jsonFileName}.json");
    }
}
