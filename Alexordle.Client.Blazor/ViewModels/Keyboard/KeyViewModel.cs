using Alexordle.Client.Application.Database.Models;
using Alexordle.Client.Application.Services;
using Alexordle.Client.Blazor.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;

namespace Alexordle.Client.Blazor.ViewModels.Keyboard;
public partial class KeyViewModel : ObservableObject, INotificationHandler<PuzzleUpdateNotification>
{
    private readonly IMediator _mediator;
    private readonly IHintService _hintService;

    public KeyViewModel(
        IMediator mediator,
        IHintService hintService)
    {
        _mediator = mediator;
        _hintService = hintService;
    }

    private char? InvariantCharacter { get; set; }

    [ObservableProperty]
    private string? _text;

    [ObservableProperty]
    private bool _isWide;

    [ObservableProperty]
    private Hints _hint;

    public void Create(string text, bool isWide, char? invariantCharacter)
    {
        Text = text;
        IsWide = isWide;
        InvariantCharacter = invariantCharacter;
    }

    public async Task Handle(PuzzleUpdateNotification notification, CancellationToken cancellationToken)
    {
        await UpdateAsync(notification.PuzzleId);
    }

    private async Task UpdateAsync(Guid puzzleId)
    {
        if (InvariantCharacter is not null)
        {
            Hint = await _hintService.GetHighestHintAsync(puzzleId, InvariantCharacter.Value);
        }
    }

    [RelayCommand]
    private async Task SubmitKeyAsync()
    {
        if (Text is KeyboardViewModel.ENTER_KEY)
        {
            await EnterAsync();
        }
        else if (Text is KeyboardViewModel.BACKSPACE_KEY)
        {
            await BackspaceAsync();
        }
        else if (InvariantCharacter is not null)
        {
            await SubmitAsync(InvariantCharacter.Value);
        }
    }

    private async Task EnterAsync()
    {
        await _mediator.Publish(new KeyEnterNotification());
    }

    private async Task BackspaceAsync()
    {
        await _mediator.Publish(new KeyBackspaceNotification());
    }

    private async Task SubmitAsync(char character)
    {
        await _mediator.Publish(new KeySubmitNotification(character));
    }
}
