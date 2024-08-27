using Alexordle.Client.Application.Exceptions;
using Alexordle.Client.Application.Services;
using Alexordle.Client.Blazor.Models;
using Alexordle.Client.Blazor.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using Lexicom.Mvvm;
using MediatR;
using System.Collections.ObjectModel;

namespace Alexordle.Client.Blazor.ViewModels.Keyboard;
public partial class KeyboardViewModel : ObservableObject, INotificationHandler<KeySubmitNotification>, INotificationHandler<KeyEnterNotification>, INotificationHandler<KeyBackspaceNotification>
{
    public const string ENTER_KEY = "Enter";
    public const string BACKSPACE_KEY = "⌫";

    private const string ROW_1 = "q,w,e,r,t,y,u,i,o,p";
    private const string ROW_2 = "a,s,d,f,g,h,j,k,l";
    private const string ROW_3 = $"{BACKSPACE_KEY},z,x,c,v,b,n,m,{ENTER_KEY}";

    private readonly IMediator _mediator;
    private readonly IViewModelFactory _viewModelFactory;
    private readonly IHunchService _hunchService;

    public KeyboardViewModel(
        IMediator mediator,
        IViewModelFactory viewModelFactory,
        IHunchService hunchService)
    {
        _mediator = mediator;
        _viewModelFactory = viewModelFactory;
        _hunchService = hunchService;

        Row1KeyViewModels = [];
        Row2KeyViewModels = [];
        Row3KeyViewModels = [];
    }

    private Guid? PuzzleId { get; set; }

    [ObservableProperty]
    private ObservableCollection<KeyViewModel> _row1KeyViewModels;

    [ObservableProperty]
    private ObservableCollection<KeyViewModel> _row2KeyViewModels;

    [ObservableProperty]
    private ObservableCollection<KeyViewModel> _row3KeyViewModels;

    public void Create(Guid puzzleId)
    {
        PuzzleId = puzzleId;

        Row1KeyViewModels.Clear();
        Row2KeyViewModels.Clear();
        Row3KeyViewModels.Clear();

        CreateKeys(ROW_1, Row1KeyViewModels);
        CreateKeys(ROW_2, Row2KeyViewModels);
        CreateKeys(ROW_3, Row3KeyViewModels, widen: true);
    }

    public async Task Handle(KeySubmitNotification notification, CancellationToken cancellationToken)
    {
        await SubmitAsync(notification.InvariantCharacter);
    }

    public async Task Handle(KeyEnterNotification notification, CancellationToken cancellationToken)
    {
        await EnterAsync();
    }

    public async Task Handle(KeyBackspaceNotification notification, CancellationToken cancellationToken)
    {
        await BackspaceAsync();
    }

    private void CreateKeys(string allKeysString, ObservableCollection<KeyViewModel> keyViewModels, bool widen = false)
    {
        string[] keys = allKeysString.Split(',');

        for (int i = 0; i < keys.Length; i++)
        {
            string text = keys[i];

            bool isWide = widen && (i is 0 || i == keys.Length - 1);

            var keyViewModel = _viewModelFactory.Create<KeyViewModel>();

            char? invariantCharacter = text.Length is 1 ? char.ToUpperInvariant(text[0]) : null;

            keyViewModel.Create(text, isWide, invariantCharacter);

            keyViewModels.Add(keyViewModel);
        }
    }

    private async Task EnterAsync()
    {
        if (PuzzleId is not null)
        {
            try
            {
                await _hunchService.SubmitHunchAsync(PuzzleId.Value);

                await _mediator.Publish(new PuzzleUpdateNotification(PuzzleId.Value));
            }
            catch (IncompleteGuessException)
            {
                await _mediator.Publish(new SetMessageNotification(PuzzleId.Value, Message.IncompeleteGuess));
            }
            catch (IncorrectSpellingException)
            {
                await _mediator.Publish(new SetMessageNotification(PuzzleId.Value, Message.IncorrectSpelling));
            }
            catch (DuplicateGuessException)
            {
                await _mediator.Publish(new SetMessageNotification(PuzzleId.Value, Message.AlreadyGuessed));
            }
        }
    }

    private async Task BackspaceAsync()
    {
        if (PuzzleId is not null)
        {
            await _hunchService.RemoveCharacterFromHunchAsync(PuzzleId.Value);

            await _mediator.Publish(new PuzzleUpdateNotification(PuzzleId.Value));
        }
    }

    private async Task SubmitAsync(char invariantCharacter)
    {
        if (PuzzleId is not null)
        {
            await _hunchService.AppendCharacterToHunchAsync(PuzzleId.Value, invariantCharacter);

            await _mediator.Publish(new PuzzleUpdateNotification(PuzzleId.Value));
        }
    }
}
