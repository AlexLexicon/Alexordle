using Alexordle.Client.Application.Database.Entities;
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
    private readonly IGuessService _guessService;
    private readonly ILetterService _letterService;
    private readonly IDictionaryService _dictionaryService;

    public KeyboardViewModel(
        IMediator mediator,
        IViewModelFactory viewModelFactory,
        IGuessService guessService,
        ILetterService letterService,
        IDictionaryService dictionaryService)
    {
        _mediator = mediator;
        _viewModelFactory = viewModelFactory;
        _guessService = guessService;
        _letterService = letterService;
        _dictionaryService = dictionaryService;

        Row1KeyViewModels = [];
        Row2KeyViewModels = [];
        Row3KeyViewModels = [];
    }

    private Guid? PuzzleId { get; set; }
    private int Width { get; set; }
    private bool IsSpellChecking { get; set; }
    private Guid? GuessWordId { get; set; }

    [ObservableProperty]
    private ObservableCollection<KeyViewModel> _row1KeyViewModels;

    [ObservableProperty]
    private ObservableCollection<KeyViewModel> _row2KeyViewModels;

    [ObservableProperty]
    private ObservableCollection<KeyViewModel> _row3KeyViewModels;

    public void Create(Puzzle puzzle)
    {
        PuzzleId = puzzle.Id;
        Width = puzzle.Width;
        IsSpellChecking = puzzle.IsSpellChecking;

        Row1KeyViewModels.Clear();
        Row2KeyViewModels.Clear();
        Row3KeyViewModels.Clear();

        AddKeys(ROW_1, Row1KeyViewModels);
        AddKeys(ROW_2, Row2KeyViewModels);
        AddKeys(ROW_3, Row3KeyViewModels, widen: true);
    }

    private void AddKeys(string allKeysString, ObservableCollection<KeyViewModel> keyViewModels, bool widen = false)
    {
        if (PuzzleId is not null)
        {
            string[] keys = allKeysString.Split(',');

            for (int i = 0; i < keys.Length; i++)
            {
                string text = keys[i];

                bool isWide = widen && (i is 0 || i == keys.Length - 1);

                var keyViewModel = _viewModelFactory.Create<KeyViewModel, string, bool>(text, isWide);

                keyViewModels.Add(keyViewModel);
            }
        }
    }

    public async Task Handle(KeySubmitNotification notification, CancellationToken cancellationToken)
    {
        await SubmitAsync(notification.Character);
    }

    public async Task Handle(KeyEnterNotification notification, CancellationToken cancellationToken)
    {
        await EnterAsync();
    }

    public async Task Handle(KeyBackspaceNotification notification, CancellationToken cancellationToken)
    {
        await BackspaceAsync();
    }

    private async Task EnterAsync()
    {
        if (PuzzleId is not null && GuessWordId is not null)
        {
            SpecialMessages? specialMessage = null;

            if (IsSpellChecking)
            {
                bool isSpelledCorrectly = await _dictionaryService.CheckGuessSpellingAsync(GuessWordId.Value);
                if (!isSpelledCorrectly)
                {
                    specialMessage = SpecialMessages.NotSpelledCorrectly;
                }
            }

            bool isAlreadyGuessed = await _guessService.IsAlreadyGuessedAsync(GuessWordId.Value);
            if (isAlreadyGuessed)
            {
                specialMessage = SpecialMessages.AlreadyGuessed;
            }

            bool isComplete = await _guessService.IsGuessCompleteAsync(GuessWordId.Value);
            if (!isComplete)
            {
                specialMessage = SpecialMessages.IncompeleteWord;
            }

            if (specialMessage is null)
            {
                await _guessService.SubmitGuessAsync(GuessWordId.Value);

                await CreateGuessAsync();
            }

            await _mediator.Publish(new StateChangedNotification(specialMessage));
        }
    }

    private async Task BackspaceAsync()
    {
        if (PuzzleId is not null && GuessWordId is not null)
        {
            await _letterService.RemoveLetterAsync(GuessWordId.Value);

            await _mediator.Publish(new StateChangedNotification());
        }
    }

    private async Task SubmitAsync(char character)
    {
        if (GuessWordId is null)
        {
            await CreateGuessAsync();
        }

        if (PuzzleId is not null && GuessWordId is not null)
        {
            int count = await _letterService.GetLettersCountAsync(GuessWordId.Value);

            if (count < Width)
            {
                await _letterService.AppendLetterAsync(GuessWordId.Value, character);

                await _mediator.Publish(new StateChangedNotification());
            }
        }
    }

    private async Task CreateGuessAsync()
    {
        if (PuzzleId is not null)
        {
            Guess guess = await _guessService.CreateGuessAsync(PuzzleId.Value);

            GuessWordId = guess.WordId;
        }
    }
}
