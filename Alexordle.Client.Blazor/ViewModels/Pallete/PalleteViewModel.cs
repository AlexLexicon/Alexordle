using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Services;
using Alexordle.Client.Blazor.Models;
using Alexordle.Client.Blazor.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using Lexicom.Mvvm;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace Alexordle.Client.Blazor.ViewModels.Pallete;
public partial class PalleteViewModel : ObservableObject, INotificationHandler<PalleteUpdateNotification>
{
    private readonly ILogger<PalleteViewModel> _logger;
    private readonly IMediator _mediator;
    private readonly IViewModelFactory _viewModelFactory;
    private readonly IPuzzleService _puzzleService;
    private readonly IClueService _clueService;
    private readonly IGuessService _guessService;
    private readonly IHunchService _hunchService;

    public PalleteViewModel(
        ILogger<PalleteViewModel> logger,
        IMediator mediator,
        IViewModelFactory viewModelFactory,
        IPuzzleService puzzleService,
        IClueService clueService,
        IGuessService guessService,
        IHunchService hunchService,
        RowViewModel hunchRowViewModel)
    {
        _logger = logger;
        _mediator = mediator;
        _viewModelFactory = viewModelFactory;
        _puzzleService = puzzleService;
        _clueService = clueService;
        _guessService = guessService;
        _hunchService = hunchService;

        ClueRowViewModels = [];
        GuessRowViewModels = [];
        RemainingRowViewModels = [];
        HunchRowViewModel = hunchRowViewModel;
    }

    private Guid? PuzzleId { get; set; }
    private int LastGuessRow { get; set; }

    [ObservableProperty]
    private ObservableCollection<RowViewModel> _clueRowViewModels;

    [ObservableProperty]
    private ObservableCollection<RowViewModel> _guessRowViewModels;

    [ObservableProperty]
    private ObservableCollection<RowViewModel> _remainingRowViewModels;

    [ObservableProperty]
    private RowViewModel _hunchRowViewModel;

    [ObservableProperty]
    private bool _isFaulted;

    [ObservableProperty]
    private int _width;

    [ObservableProperty]
    private bool _isFinished;

    public async Task Handle(PalleteUpdateNotification notification, CancellationToken cancellationToken)
    {
        await GenerateAsync(notification.PuzzleId);
    }

    private async Task GenerateAsync(Guid puzzleId)
    {
        try
        {
            IsFaulted = false;

            Puzzle puzzle = await _puzzleService.GetPuzzleAsync(puzzleId);
            IsFinished = puzzle.IsFinished;

            int row;
            bool isComplete = false;
            if (puzzleId != PuzzleId || Width != puzzle.Width)
            {
                ClueRowViewModels.Clear();
                GuessRowViewModels.Clear();
                RemainingRowViewModels.Clear();

                Width = puzzle.Width;

                row = 0;
                while (!isComplete)
                {
                    IReadOnlyList<ClueCharacter> clues = await _clueService.GetCluesAsync(puzzleId, row);

                    isComplete = clues.Count is <= 0;
                    if (!isComplete)
                    {
                        RowViewModel clueRowViewModel = _viewModelFactory.Create<RowViewModel>();

                        clueRowViewModel.Set(Width, clues);

                        ClueRowViewModels.Add(clueRowViewModel);

                        row++;
                    }
                }

                LastGuessRow = 0;
                PuzzleId = puzzleId;
            }

            row = LastGuessRow;
            isComplete = false;
            while (!isComplete)
            {
                IReadOnlyList<GuessCharacter> guesses = await _guessService.GetGuessesAsync(puzzleId, row);

                isComplete = guesses.Count is <= 0;
                if (!isComplete)
                {
                    RowViewModel guessRowViewModel = _viewModelFactory.Create<RowViewModel>();

                    guessRowViewModel.Set(Width, guesses);

                    GuessRowViewModels.Add(guessRowViewModel);

                    row++;
                }
            }

            int remaining = puzzle.MaxGuesses is null ? 0 : puzzle.MaxGuesses.Value - puzzle.CurrentGuesses - (!IsFinished ? 1 : 0);
            if (remaining != RemainingRowViewModels.Count)
            {
                RemainingRowViewModels.Clear();
                for (int count = 0; count < remaining; count++)
                {
                    RowViewModel remainingRowViewModel = _viewModelFactory.Create<RowViewModel>();

                    remainingRowViewModel.Set(Width);

                    RemainingRowViewModels.Add(remainingRowViewModel);
                }
            }

            IReadOnlyList<HunchCharacter> hunches = await _hunchService.GetHunchesAsync(puzzleId);

            HunchRowViewModel.Set(puzzleId, Width, hunches);

            LastGuessRow = row;

            await _mediator.Publish(new SetMessageNotification(puzzleId, Message.Default));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An unexpected exception occured while generating the pallete for the puzzle with the id '{puzzleId}'.", puzzleId);

            IsFaulted = true;
        }
    }
}
