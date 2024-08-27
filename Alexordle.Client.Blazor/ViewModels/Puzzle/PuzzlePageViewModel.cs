using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Services;
using Alexordle.Client.Blazor.Exceptions;
using Alexordle.Client.Blazor.Models;
using Alexordle.Client.Blazor.Notifications;
using Alexordle.Client.Blazor.ViewModels.Pallete;
using Alexordle.Client.Blazor.ViewModels.Keyboard;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lexicom.Concentrate.Blazor.WebAssembly.Amenities.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Alexordle.Client.Blazor.ViewModels.Game;
public partial class PuzzlePageViewModel : ObservableObject, INotificationHandler<PuzzleUpdateNotification>//, INotificationHandler<StateChangedNotification>
{
    private readonly ILogger<PuzzlePageViewModel> _logger;
    private readonly IMediator _mediator;
    private readonly ITransmissionService _transmissionService;
    private readonly INavigationService _navigationService;
    private readonly IClipboardService _clipboardService;
    private readonly IShareService _shareService;
    private readonly IAnswerService _answerService;
    private readonly IClueService _clueService;
    private readonly IPuzzleService _puzzleService;

    public PuzzlePageViewModel(
        ILogger<PuzzlePageViewModel> logger,
        IMediator mediator,
        ITransmissionService transmissionService,
        INavigationService navigationService,
        IClipboardService clipboardService,
        IShareService shareService,
        IAnswerService answerService,
        IClueService clueService,
        IPuzzleService puzzleService,
        PalleteViewModel palleteViewModel,
        KeyboardViewModel keyboardViewModel,
        MessageViewModel messageViewModel)
    {
        _logger = logger;
        _mediator = mediator;
        _transmissionService = transmissionService;
        _navigationService = navigationService;
        _clipboardService = clipboardService;
        _shareService = shareService;
        _answerService = answerService;
        _clueService = clueService;
        _puzzleService = puzzleService;

        PalleteViewModel = palleteViewModel;
        KeyboardViewModel = keyboardViewModel;
        MessageViewModel = messageViewModel;
    }

    public string? QueryString { get; set; }

    private Guid? PuzzleId { get; set; }

    [ObservableProperty]
    private PalleteViewModel _palleteViewModel;

    [ObservableProperty]
    private KeyboardViewModel _keyboardViewModel;

    [ObservableProperty]
    private MessageViewModel _messageViewModel;

    [ObservableProperty]
    private bool _isFaulted;

    [ObservableProperty]
    private string? _explanation;

    [ObservableProperty]
    private bool _isComplete;

    public async Task Handle(PuzzleUpdateNotification notification, CancellationToken cancellationToken)
    {
        Puzzle puzzle = await _puzzleService.GetPuzzleAsync(notification.PuzzleId);

        IsComplete = puzzle.IsComplete;
    }

    [RelayCommand]
    private async Task Rendered(bool isFirstRender)
    {
        if (isFirstRender)
        {
            try
            {
                if (QueryString is null)
                {
                    throw new NullQueryStringException();
                }

                Puzzle puzzle = await _transmissionService.DeserializeAndStartPuzzleAsync(QueryString);

                PuzzleId = puzzle.Id;

                _logger.LogInformation("Starting Puzzle with the id '{puzzleId}'.", PuzzleId);

                KeyboardViewModel.Create(PuzzleId.Value);

                var publish = _mediator.Publish(new PuzzleUpdateNotification(PuzzleId.Value));
                var generateExplanationTask = GenerateExplanationAsync(PuzzleId.Value);

                await publish;
                await generateExplanationTask;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "The puzzle failed to load from the query string '{queryString}'.", QueryString);

                IsFaulted = true;
            }
        }
    }

    [RelayCommand]
    private async Task NavigateToDesignerAsync()
    {
        string baseUrl = await _navigationService.GetBaseUrlAsync();

        await _navigationService.NavigateToUrlAsync(baseUrl, forceLoad: true);
    }

    [RelayCommand]
    private async Task CopyShareAsync()
    {
        if (PuzzleId is not null)
        {
            string share = await _shareService.GenerateShareAsync(PuzzleId.Value);

            await _clipboardService.WriteAsync(share);

            await _mediator.Publish(new SetMessageNotification(PuzzleId.Value, Message.CopiedShare));
        }
    }

    private async Task GenerateExplanationAsync(Guid puzzleId)
    {
        string explanation = string.Empty;

        var getClueInvariantTextsTask = _clueService.GetClueInvariantTextsAsync(puzzleId);
        var getAnswerInvariantTextsTask = _answerService.GetAnswerInvariantTextsAsync(puzzleId);

        IReadOnlyList<string> clues = await getClueInvariantTextsTask;
        for (int i = 0; i < clues.Count; i++)
        {
            if (i is > 0)
            {
                explanation += " / ";
            }

            explanation += clues[i];
        }

        if (clues.Count > 0)
        {
            explanation += " » ";
        }

        IReadOnlyList<string> answers = await getAnswerInvariantTextsTask;
        for (int i = 0; i < answers.Count; i++)
        {
            if (i is > 0)
            {
                explanation += " + ";
            }

            explanation += answers[i];
        }

        Explanation = explanation;
    }

    //private readonly ILogger<GamePageViewModel> _logger;
    //private readonly IMediator _mediator;
    //private readonly IGameService _gameService;
    //private readonly INavigationService _navigationService;
    //private readonly IPuzzleService _puzzleService;
    //private readonly IStateService _stateService;
    //private readonly IAnswerService _answerService;
    //private readonly IClueService _clueService;
    //private readonly IShareService _shareService;
    //private readonly IClipboardService _clipboardService;
    //private readonly ITimeProvider _timeProvider;

    //public GamePageViewModel(
    //    ILogger<GamePageViewModel> logger,
    //    IMediator mediator,
    //    IGameService gameService,
    //    INavigationService navigationService,
    //    IPuzzleService puzzleService,
    //    IStateService stateService,
    //    IAnswerService answerService,
    //    IClueService clueService,
    //    IShareService shareService,
    //    IClipboardService clipboardService,
    //    ITimeProvider timeProvider,
    //    KeyboardViewModel keyboardViewModel,
    //    MessageViewModel messageViewModel,
    //    PalleteViewModel palleteViewModel)
    //{
    //    _logger = logger;
    //    _mediator = mediator;
    //    _gameService = gameService;
    //    _navigationService = navigationService;
    //    _puzzleService = puzzleService;
    //    _stateService = stateService;
    //    _answerService = answerService;
    //    _clueService = clueService;
    //    _shareService = shareService;
    //    _clipboardService = clipboardService;
    //    _timeProvider = timeProvider;

    //    KeyboardViewModel = keyboardViewModel;
    //    MessageViewModel = messageViewModel;
    //    PalleteViewModel = palleteViewModel;
    //    PalleteViewModel.IsLoadable = false;
    //}

    //public string? PuzzleCode { get; set; }
    //private string? PuzzleCodeStore { get; set; }
    //private Guid? PuzzleId { get; set; }

    //[ObservableProperty]
    //private PalleteViewModel _palleteViewModel;

    //[ObservableProperty]
    //private KeyboardViewModel _keyboardViewModel;

    //[ObservableProperty]
    //private MessageViewModel _messageViewModel;

    //[ObservableProperty]
    //private bool _isFinished;

    //[ObservableProperty]
    //private string? _explaination;

    //[ObservableProperty]
    //private bool _isFailed;

    //[RelayCommand]
    //private async Task NavigateToDesignerAsync()
    //{
    //    string baseUrl = await _navigationService.GetBaseUrlAsync();

    //    await _navigationService.NavigateToUrlAsync(baseUrl, forceLoad: true);
    //}

    //[RelayCommand]
    //private async Task Rendered()
    //{
    //    if (!IsFailed && PuzzleId is null && PuzzleCode is not null)
    //    {
    //        try
    //        {
    //            PalleteViewModel.IsReload = true;

    //            IsFailed = false;

    //            PuzzleCodeStore = PuzzleCode;

    //            PuzzleId = await _gameService.LoadPuzzleFromCodeAsync(PuzzleCode);

    //            Puzzle puzzle = await _puzzleService.GetPuzzleAsync(PuzzleId.Value);

    //            IReadOnlyList<Clue> clues = await _clueService.GetCluesAsync(PuzzleId.Value);

    //            KeyboardViewModel.Create(puzzle);

    //            await CalculateExplanationAsync();

    //            await UpdateGameAsync();
    //        }
    //        catch (Exception e)
    //        {
    //            _logger.LogError(e, "Failed to load the game.");

    //            IsFailed = true;
    //        }
    //    }
    //}

    //[RelayCommand]
    //private async Task ShareAsync()
    //{
    //    if (PuzzleId is not null && PuzzleCodeStore is not null)
    //    {
    //        await UpdateGameAsync(SpecialMessages.ShareCopied);

    //        string share = await _shareService.GenerateShareAsync(PuzzleId.Value, PuzzleCodeStore);

    //        await _clipboardService.WriteAsync(share);
    //    }
    //}

    //public async Task Handle(StateChangedNotification notification, CancellationToken cancellationToken)
    //{
    //    await UpdateGameAsync(notification.SpecialMessage);
    //}

    //private async Task UpdateGameAsync(SpecialMessages? specialMessage = null)
    //{
    //    if (PuzzleId is not null)
    //    {
    //        State state = await _stateService.GetStateAsync(PuzzleId.Value);

    //        IsFinished = state.IsVictory || state.IsDefeat;

    //        long tick = _timeProvider.GetUtcNow().Ticks;

    //        await _mediator.Publish(new GeneratePalleteNotification(tick, PuzzleId.Value, state, specialMessage));
    //    }
    //}

    //private async Task CalculateExplanationAsync()
    //{
    //    if (PuzzleId is not null)
    //    {
    //        var getAnswersTask = _answerService.GetAnswersAsync(PuzzleId.Value);
    //        var getCluesTask = _clueService.GetCluesAsync(PuzzleId.Value);

    //        string explaination = string.Empty;

    //        IReadOnlyList<Clue> clues = await getCluesTask;
    //        for (int i = 0; i < clues.Count; i++)
    //        {
    //            if (i is > 0)
    //            {
    //                explaination += " / ";
    //            }

    //            explaination += clues[i].InvariantCharacter;
    //        }

    //        if (clues.Count > 0)
    //        {
    //            explaination += " » ";
    //        }

    //        IReadOnlyList<Answer> answers = await getAnswersTask;
    //        for (int i = 0; i < answers.Count; i++)
    //        {
    //            if (i is > 0)
    //            {
    //                explaination += " + ";
    //            }

    //            explaination += answers[i].InvariantText;
    //        }

    //        Explaination = explaination;
    //    }
    //}
}
