using Alexordle.Client.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lexicom.Concentrate.Blazor.WebAssembly.Amenities.Services;

namespace Alexordle.Client.Blazor.ViewModels;
public partial class FooterViewModel : ObservableObject
{
    private readonly IPersistenceService _persistenceService;
    private readonly INavigationService _navigationService;

    public FooterViewModel(
        IPersistenceService persistenceService, 
        INavigationService navigationService)
    {
        _persistenceService = persistenceService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    private async Task ClearDataAsync()
    {
        await _persistenceService.ClearDataAsync();

        await _navigationService.RefreshPageAsync();
    }
}
