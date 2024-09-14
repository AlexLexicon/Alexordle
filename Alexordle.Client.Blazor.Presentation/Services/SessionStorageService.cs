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

    public async Task ClearAsync()
    {
        await _localStorageService.ClearAsync();
    }

    public async Task StoreAsync(string key, string value)
    {
        await _localStorageService.SetItemAsStringAsync(key, value);
    }

    public async Task<string?> FetchAsync(string key)
    {
        return await _localStorageService.GetItemAsStringAsync(key);
    }
}
