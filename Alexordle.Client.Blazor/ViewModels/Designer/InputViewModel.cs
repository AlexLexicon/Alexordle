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
    }

    [ObservableProperty]
    private string? _label;

    [ObservableProperty]
    private string? _text;

    [ObservableProperty]
    private IRuleSetValidator<string?> _textValidator;

    public async Task UpdateValidatorAsync()
    {
        await TextValidator.ValidateAsync(Text);
    }

    [RelayCommand]
    private async Task TextChangedAsync()
    {
        await _mediator.Publish(new DesignChangedNotification());
    }
}
