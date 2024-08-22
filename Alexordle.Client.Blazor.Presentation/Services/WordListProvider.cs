using Alexordle.Client.Application.Services;
using Lexicom.Http.Extensions;

namespace Alexordle.Client.Blazor.Presentation.Services;
public class WordListProvider : IWordListProvider
{
    private readonly IHttpClientFactory _httpClientFactory;

    public WordListProvider(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HashSet<string>> LoadWords5Async()
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("wwwroot");

        return await httpClient.GetFromJsonNotNullAsync<HashSet<string>>("words5.json");
    }
}
