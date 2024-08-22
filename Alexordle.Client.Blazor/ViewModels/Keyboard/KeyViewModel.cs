using Alexordle.Client.Application.Extensions;
using Alexordle.Client.Application.Models;
using Alexordle.Client.Application.Services;
using Alexordle.Client.Blazor.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;

namespace Alexordle.Client.Blazor.ViewModels.Keyboard;
public partial class KeyViewModel : ObservableObject, INotificationHandler<CharacterHighlightNotification>
{
    private readonly IMediator _mediator;

    public KeyViewModel(
        string text,
        bool isWide,
        IMediator mediator)
    {
        _mediator = mediator;

        InvariantCharacter = text.Length is 1 ? LetterService.MakeInvariant(text[0]) : null;

        Text = text;
        IsWide = isWide;
    }

    private char? InvariantCharacter { get; }

    [ObservableProperty]
    private string? _text;

    [ObservableProperty]
    private bool _isWide;

    [ObservableProperty]
    private Highlights _highlight;

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

    public Task Handle(CharacterHighlightNotification notification, CancellationToken cancellationToken)
    {
        if (InvariantCharacter is not null && InvariantCharacter == notification.Character && notification.Highlight is not Highlights.None)
        {
            if (notification.IsFinished)
            {
                Highlight = Highlights.CommittedIncorrect;
            }
            else
            {
                Highlight = Highlight.Better(notification.Highlight);
            }
        }

        return Task.CompletedTask;
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
