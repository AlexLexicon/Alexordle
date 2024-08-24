using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Models;
using Alexordle.Client.Application.Services;
using Alexordle.Client.Blazor.Models;
using Alexordle.Client.Blazor.Notifications;
using Alexordle.Client.Blazor.ViewModels.Grid;
using Alexordle.Client.Blazor.ViewModels.Keyboard;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lexicom.Concentrate.Blazor.WebAssembly.Amenities.Services;
using Lexicom.DependencyInjection.Primitives;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Alexordle.Client.Blazor.ViewModels.Game;
public partial class GamePageViewModel : ObservableObject, INotificationHandler<StateChangedNotification>
{
    private readonly ILogger<GamePageViewModel> _logger;
    private readonly IMediator _mediator;
    private readonly IGameService _gameService;
    private readonly INavigationService _navigationService;
    private readonly IPuzzleService _puzzleService;
    private readonly IStateService _stateService;
    private readonly IAnswerService _answerService;
    private readonly IClueService _clueService;
    private readonly IShareService _shareService;
    private readonly IClipboardService _clipboardService;
    private readonly ITimeProvider _timeProvider;

    public GamePageViewModel(
        ILogger<GamePageViewModel> logger,
        IMediator mediator,
        IGameService gameService,
        INavigationService navigationService,
        IPuzzleService puzzleService,
        IStateService stateService,
        IAnswerService answerService,
        IClueService clueService,
        IShareService shareService,
        IClipboardService clipboardService,
        ITimeProvider timeProvider,
        KeyboardViewModel keyboardViewModel,
        MessageViewModel messageViewModel,
        PalleteViewModel palleteViewModel)
    {
        _logger = logger;
        _mediator = mediator;
        _gameService = gameService;
        _navigationService = navigationService;
        _puzzleService = puzzleService;
        _stateService = stateService;
        _answerService = answerService;
        _clueService = clueService;
        _shareService = shareService;
        _clipboardService = clipboardService;
        _timeProvider = timeProvider;

        KeyboardViewModel = keyboardViewModel;
        MessageViewModel = messageViewModel;
        PalleteViewModel = palleteViewModel;
        PalleteViewModel.IsLoadable = false;
    }

    public string? PuzzleCode { get; set; }
    private string? PuzzleCodeStore { get; set; }
    private Guid? PuzzleId { get; set; }

    [ObservableProperty]
    private PalleteViewModel _palleteViewModel;

    [ObservableProperty]
    private KeyboardViewModel _keyboardViewModel;

    [ObservableProperty]
    private MessageViewModel _messageViewModel;

    [ObservableProperty]
    private bool _isFinished;

    [ObservableProperty]
    private bool _isExplainable;

    [ObservableProperty]
    private string? _explaination;

    [ObservableProperty]
    private bool _isFailed;

    [RelayCommand]
    private async Task NavigateToDesignerAsync()
    {
        string baseUrl = await _navigationService.GetBaseUrlAsync();

        await _navigationService.NavigateToUrlAsync(baseUrl, forceLoad: true);
    }

    [RelayCommand]
    private async Task Rendered()
    {
        if (!IsFailed && PuzzleId is null && PuzzleCode is not null)
        {
            try
            {
                PalleteViewModel.IsReload = true;

                IsFailed = false;

                PuzzleCodeStore = PuzzleCode;

                PuzzleId = await _gameService.LoadPuzzleFromCodeAsync(PuzzleCode);

                Puzzle puzzle = await _puzzleService.GetPuzzleAsync(PuzzleId.Value);

                IReadOnlyList<Clue> clues = await _clueService.GetCluesAsync(PuzzleId.Value);

                IsExplainable = clues.Count is > 0;

                KeyboardViewModel.Create(puzzle);

                await CalculateExplanationAsync();

                await UpdateGameAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to load the game.");

                IsFailed = true;
            }
        }
    }

    [RelayCommand]
    private async Task ShareAsync()
    {
        if (PuzzleId is not null && PuzzleCodeStore is not null)
        {
            await UpdateGameAsync(SpecialMessages.ShareCopied);

            string share = await _shareService.GenerateShareAsync(PuzzleId.Value, PuzzleCodeStore);

            await _clipboardService.WriteAsync(share);
        }
    }

    public async Task Handle(StateChangedNotification notification, CancellationToken cancellationToken)
    {
        await UpdateGameAsync(notification.SpecialMessage);
    }

    private async Task UpdateGameAsync(SpecialMessages? specialMessage = null)
    {
        if (PuzzleId is not null)
        {
            State state = await _stateService.GetStateAsync(PuzzleId.Value);

            IsFinished = state.IsVictory || state.IsDefeat;

            long tick = _timeProvider.GetUtcNow().Ticks;

            await _mediator.Publish(new GeneratePalleteNotification(tick, PuzzleId.Value, state, specialMessage));
        }
    }

    private async Task CalculateExplanationAsync()
    {
        if (PuzzleId is not null)
        {
            var getAnswersTask = _answerService.GetAnswersAsync(PuzzleId.Value);
            var getCluesTask = _clueService.GetCluesAsync(PuzzleId.Value);

            string explaination = string.Empty;

            IReadOnlyList<Clue> clues = await getCluesTask;
            for (int i = 0; i < clues.Count; i++)
            {
                if (i is > 0)
                {
                    explaination += " / ";
                }

                explaination += clues[i].InvariantText;
            }

            explaination += " » ";

            IReadOnlyList<Answer> answers = await getAnswersTask;
            for (int i = 0; i < answers.Count; i++)
            {
                if (i is > 0)
                {
                    explaination += " + ";
                }

                explaination += answers[i].InvariantText;
            }

            Explaination = explaination;
        }
    }
}
