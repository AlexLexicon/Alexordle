using Alexordle.Client.Application.Services;
using Lexicom.Concentrate.Blazor.WebAssembly.Amenities.Services;

namespace Alexordle.Client.Blazor.Presentation.Services;
public class UrlService : IUrlService
{
    private readonly INavigationService _navigationService;

    public UrlService(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public Task<string> GetUrlAsync()
    {
        return _navigationService.GetBaseUrlAsync();
    }

    public async Task<string> GetPuzzleUrlAsync(string code)
    {
        string url = await _navigationService.GetUrlAsync();

        return $"{url}game?puzzle={code}";
    }
}
