using Alexordle.Client.Application.Services;
using Blazored.LocalStorage;

namespace Alexordle.Client.Blazor.Presentation.Services;
public class SessionStorageService : IStorageService
{
    private readonly ILocalStorageService _localStorageService;

    public SessionStorageService(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }

    public async Task Store(string key, string value)
    {
        await _localStorageService.SetItemAsStringAsync(key, value);
    }

    public async Task<string?> Fetch(string key)
    {
        return await _localStorageService.GetItemAsStringAsync(key);
    }
}
