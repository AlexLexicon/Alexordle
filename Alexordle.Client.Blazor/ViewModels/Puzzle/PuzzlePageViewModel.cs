using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Exceptions;
using Alexordle.Client.Application.Services;
using Alexordle.Client.Blazor.Exceptions;
using Alexordle.Client.Blazor.Models;
using Alexordle.Client.Blazor.Notifications;
using Alexordle.Client.Blazor.ViewModels.Keyboard;
using Alexordle.Client.Blazor.ViewModels.Pallete;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lexicom.Concentrate.Blazor.WebAssembly.Amenities.Notifications;
using Lexicom.Concentrate.Blazor.WebAssembly.Amenities.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Alexordle.Client.Blazor.ViewModels.Game;
public partial class PuzzlePageViewModel : ObservableObject, INotificationHandler<PuzzleSubmitNotification>, INotificationHandler<KeySubmitNotification>, INotificationHandler<KeyEnterNotification>, INotificationHandler<KeyBackspaceNotification>
{
    private readonly ILogger<PuzzlePageViewModel> _logger;
    private readonly IMediator _mediator;
    private readonly ISerializationService _serializationService;
    private readonly INavigationService _navigationService;
    private readonly IClipboardService _clipboardService;
    private readonly IShareService _shareService;
    private readonly IAnswerService _answerService;
    private readonly IClueService _clueService;
    private readonly IPuzzleService _puzzleService;
    private readonly IPersistenceService _persistenceService;
    private readonly IHunchService _hunchService;
    private readonly IKeyboardService _keyboardService;

    public PuzzlePageViewModel(
        ILogger<PuzzlePageViewModel> logger,
        IMediator mediator,
        ISerializationService serializationService,
        INavigationService navigationService,
        IClipboardService clipboardService,
        IShareService shareService,
        IAnswerService answerService,
        IClueService clueService,
        IPuzzleService puzzleService,
        IPersistenceService persistenceService,
        IHunchService hunchService,
        IKeyboardService keyboardService,
        PalleteViewModel palleteViewModel,
        KeyboardViewModel keyboardViewModel,
        MessageViewModel messageViewModel)
    {
        _logger = logger;
        _mediator = mediator;
        _serializationService = serializationService;
        _navigationService = navigationService;
        _clipboardService = clipboardService;
        _shareService = shareService;
        _answerService = answerService;
        _clueService = clueService;
        _puzzleService = puzzleService;
        _persistenceService = persistenceService;
        _hunchService = hunchService;
        _keyboardService = keyboardService;

        PalleteViewModel = palleteViewModel;
        KeyboardViewModel = keyboardViewModel;
        MessageViewModel = messageViewModel;
    }

    public string? QueryString { get; set; }

    private Guid? PuzzleId { get; set; }
    private string? SerializedPuzzle { get; set; }

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
    private bool _isFinished;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isInfinite;

    public async Task Handle(PuzzleSubmitNotification notification, CancellationToken cancellationToken)
    {
        await UpdateStateAsync();
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

    [RelayCommand]
    private async Task Rendered(bool isFirstRender)
    {
        if (isFirstRender)
        {
            IsLoading = true;

            try
            {
                await _keyboardService.InitalizeNotificationsAsync(reset: true);

                await _puzzleService.DeletePuzzlesAsync();

                SerializedPuzzle = QueryString;

                //the query string is automatically decoded by blazor but we want to keep it encoded.
                if (SerializedPuzzle is not null)
                {
                    SerializedPuzzle = await _serializationService.EncodeSerializedPuzzleForUrlAsync(SerializedPuzzle);
                }

                Puzzle puzzle = await LoadSerializedPuzzleAsync();

                PuzzleId = puzzle.Id;
                IsInfinite = puzzle.MaxGuesses is null;

                KeyboardViewModel.Create();

                await _mediator.Publish(new PalleteUpdateNotification(PuzzleId.Value));
                await _mediator.Publish(new PuzzleSubmitNotification(PuzzleId.Value));
                await GenerateExplanationAsync();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "The puzzle failed to load.");

                IsFaulted = true;
            }

            IsLoading = false;
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
        await AutoRecoverPuzzleAsync(async pi =>
        {
            string share = await _shareService.GenerateShareAsync(pi);

            await _clipboardService.WriteAsync(share);

            await _mediator.Publish(new SetMessageNotification(pi, Message.CopiedShare));
        });
    }

    private async Task UpdateStateAsync()
    {
        await AutoRecoverPuzzleAsync(async pi =>
        {
            Puzzle puzzle = await _puzzleService.GetPuzzleAsync(pi);

            IsFinished = puzzle.IsFinished;
        });
    }

    private async Task EnterAsync()
    {
        await AutoRecoverPuzzleAsync(async pi =>
        {
            try
            {
                await _hunchService.SubmitHunchAsync(pi);

                await _persistenceService.SavePuzzleAsync(pi);

                await _mediator.Publish(new PalleteUpdateNotification(pi));

                await _mediator.Publish(new PuzzleSubmitNotification(pi));
            }
            catch (IncompleteGuessException)
            {
                await _mediator.Publish(new SetMessageNotification(pi, Message.IncompeleteGuess));
            }
            catch (IncorrectSpellingException)
            {
                await _mediator.Publish(new SetMessageNotification(pi, Message.IncorrectSpelling));
            }
            catch (DuplicateGuessException)
            {
                await _mediator.Publish(new SetMessageNotification(pi, Message.AlreadyGuessed));
            }
        });
    }

    private async Task BackspaceAsync()
    {
        await AutoRecoverPuzzleAsync(async pi =>
        {
            await _hunchService.RemoveCharacterFromHunchAsync(pi);

            await _mediator.Publish(new PalleteUpdateNotification(pi));
        });
    }

    private async Task SubmitAsync(char invariantCharacter)
    {
        await AutoRecoverPuzzleAsync(async pi =>
        {
            await _hunchService.AppendCharacterToHunchAsync(pi, invariantCharacter);

            await _persistenceService.SavePuzzleAsync(pi);

            await _mediator.Publish(new PalleteUpdateNotification(pi));
        });
    }

    private async Task GenerateExplanationAsync()
    {
        await AutoRecoverPuzzleAsync(async pi =>
        {
            string explanation = string.Empty;

            var getClueInvariantTextsTask = _clueService.GetClueInvariantTextsAsync(pi);
            var getAnswerInvariantTextsTask = _answerService.GetAnswerInvariantTextsAsync(pi);

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
        });
    }

    private async Task AutoRecoverPuzzleAsync(Func<Guid, Task> operationDelegate)
    {
        if (PuzzleId is not null)
        {
            Guid puzzleId = PuzzleId.Value;
            for (int retry = 0; retry < 2; retry++)
            {
                try
                {
                    try
                    {
                        bool exists = await _puzzleService.PuzzleExistsAsync(puzzleId);
                        if (exists)
                        {
                            await operationDelegate(puzzleId);

                            return;
                        }
                    }
                    catch (PuzzleDoesNotExistException)
                    {

                    }

                    if (retry is not 0)
                    {
                        throw new UnableToRecoverPuzzleException(puzzleId, SerializedPuzzle);
                    }

                    //after a while the sqlite database loses all of its memory in the browser
                    //so we can load the puzzle again from local storage if it exists
                    Puzzle puzzle = await LoadSerializedPuzzleAsync();

                    puzzleId = puzzle.Id;

                    PuzzleId = puzzleId;
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "An unexpected error occured that could not be recovered from.");

                    IsFaulted = true;
                }
            }
        }
    }

    private async Task<Puzzle> LoadSerializedPuzzleAsync()
    {
        if (SerializedPuzzle is null)
        {
            throw new SerializedPuzzleIsNullException();
        }

        Puzzle? storedPuzzle = await _persistenceService.LoadPuzzleAsync(SerializedPuzzle);
        if (storedPuzzle is null)
        {
            return await _serializationService.DeserializeAndStartPuzzleAsync(SerializedPuzzle);
        }

        return storedPuzzle;
    }
}
