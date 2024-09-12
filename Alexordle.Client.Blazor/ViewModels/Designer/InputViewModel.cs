using Alexordle.Client.Blazor.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lexicom.Validation;
using MediatR;

namespace Alexordle.Client.Blazor.ViewModels.Designer;
public partial class InputViewModel : ObservableObject
{
    protected readonly IMediator _mediator;

    public InputViewModel(
        IMediator mediator,
        IRuleSetValidator<string?> textValidator)
    {
        _mediator = mediator;
        TextValidator = textValidator;

        IsInputVisible = true;
    }

    [ObservableProperty]
    private string? _label;

    [ObservableProperty]
    private string? _text;

    [ObservableProperty]
    private bool _isInputVisible;

    [ObservableProperty]
    private IRuleSetValidator<string?> _textValidator;

    public async Task<bool> CheckAndValidateAsync()
    {
        await TextValidator.ValidateAsync(Text);

        return TextValidator.IsValid;
    }

    protected async Task DesignChangedAsync()
    {
        await _mediator.Publish(new DesignChangedNotification());
    }

    [RelayCommand]
    private async Task TextChangedAsync()
    {
        await DesignChangedAsync();
    }
}
