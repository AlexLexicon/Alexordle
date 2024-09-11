using Alexordle.Client.Blazor.Notifications;
using CommunityToolkit.Mvvm.Input;
using Lexicom.Validation;
using MediatR;

namespace Alexordle.Client.Blazor.ViewModels.Designer;
public partial class ListInputViewModel : InputViewModel
{
    public ListInputViewModel(
        IRuleSetValidator<string?> textValidator, 
        IMediator mediator) : base(mediator, textValidator)
    {
    }

    [RelayCommand]
    private async Task RemoveAsync()
    {
        await _mediator.Publish(new RemoveListInputViewModelNotification(this));

        await DesignChangedAsync();
    }
}
