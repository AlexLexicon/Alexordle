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
public partial class PalleteViewModel : ObservableObject, INotificationHandler<PuzzleUpdateNotification>//, INotificationHandler<PeriodicTickNotification>
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

    public async Task Handle(PuzzleUpdateNotification notification, CancellationToken cancellationToken)
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
            if (puzzleId != PuzzleId)
            {
                ClueRowViewModels.Clear();
                GuessRowViewModels.Clear();

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

            //int remaining = puzzle. (GuessRowViewModels.Count + (!IsFinished ? 1 : 0))

            //int remainingGuesses = 1;
            //if (MaxGuesses is not null)
            //{
            //    remainingGuesses = MaxGuesses.Value - GuessRowViewModels.Count;
            //}

            //IsComplete = remainingGuesses is <= 0;

            IReadOnlyList<HunchCharacter> hunches = await _hunchService.GetHunchesAsync(puzzleId);

            HunchRowViewModel.Set(puzzleId, Width, hunches);

            //remainingGuesses--;

            //if (remainingGuesses is > 1 && RemainingRowViewModels.Count != remainingGuesses)
            //{
            //    RemainingRowViewModels.Clear();
            //    for (int count = 0; count < remainingGuesses; count++)
            //    {
            //        RowViewModel remainingRowViewModel = _viewModelFactory.Create<RowViewModel>();

            //        remainingRowViewModel.Set(Width);

            //        RemainingRowViewModels.Add(remainingRowViewModel);
            //    }
            //}

            LastGuessRow = row;

            await _mediator.Publish(new SetMessageNotification(puzzleId, Message.Default));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An unexpected exception occured while generating the pallete for the puzzle with the id '{puzzleId}'.", puzzleId);

            IsFaulted = true;
        }
    }

    //private readonly IMediator _mediator;
    //private readonly IViewModelFactory _viewModelFactory;
    //private readonly IPalleteService _palleteService;
    //private readonly IStateService _stateService;

    //public PalleteViewModel(
    //    IViewModelFactory viewModelFactory,
    //    IPalleteService palleteService,
    //    IStateService stateService,
    //    IMediator mediator)
    //{
    //    _mediator = mediator;
    //    _viewModelFactory = viewModelFactory;
    //    _palleteService = palleteService;
    //    _stateService = stateService;

    //    ClueRowViewModels = [];
    //    RowViewModels = [];
    //}

    //public bool IsReload { get; set; }
    //public bool IsLoadable { get; set; }
    //private bool IsGenerating { get; set; }
    //private long LastGeneratedUtcTick { get; set; }
    //private GeneratePalleteNotification? GeneratePalleteNotification { get; set; }

    //[ObservableProperty]
    //private ObservableCollection<RowViewModel> _clueRowViewModels;

    //[ObservableProperty]
    //private ObservableCollection<RowViewModel> _rowViewModels;

    //[ObservableProperty]
    //public bool _isLoading;

    //[ObservableProperty]
    //private bool _invalid;

    //[ObservableProperty]
    //private string? _columns;

    //public Task Handle(GeneratePalleteNotification notification, CancellationToken cancellationToken)
    //{
    //    if (notification.InitiatedUtcTick > LastGeneratedUtcTick)
    //    {
    //        if (IsLoadable)
    //        {
    //            IsLoading = true;
    //        }

    //        GeneratePalleteNotification = notification;
    //    }

    //    return Task.CompletedTask;
    //}

    //public async Task Handle(PeriodicTickNotification notification, CancellationToken cancellationToken)
    //{
    //    if (GeneratePalleteNotification is not null && !IsGenerating)
    //    {
    //        IsReload = false;
    //        IsGenerating = true;

    //        await GenerateAsync(GeneratePalleteNotification.PuzzleId, GeneratePalleteNotification.State, GeneratePalleteNotification.SpecialMessage, GeneratePalleteNotification.IsDesigner);

    //        LastGeneratedUtcTick = notification.UtcNow.Ticks;
    //        GeneratePalleteNotification = null;

    //        IsGenerating = false;
    //        IsLoading = false;
    //    }
    //}

    //[RelayCommand]
    //private void Loaded()
    //{
    //    IsLoading = true;
    //}

    //private async Task GenerateAsync(Guid puzzleId, State? state, Message? specialMessage, bool isDesigner)
    //{
    //    if (!IsReload)
    //    {
    //        ClueRowViewModels.Clear();
    //        RowViewModels.Clear();
    //        state ??= await _stateService.GetStateAsync(puzzleId);
    //        Pallete pallete = await _palleteService.GeneratePalleteAsync(puzzleId, state, isDesigner);

    //        Invalid = pallete.Width is <= 0;
    //        if (!Invalid)
    //        {
    //            Columns = $"{pallete.Width + 2}";

    //            foreach (Row row in pallete.Clues)
    //            {
    //                var rowViewModel = _viewModelFactory.Create<RowViewModel, Row>(row);

    //                await rowViewModel.CreateAsync(pallete.Width);

    //                ClueRowViewModels.Add(rowViewModel);
    //            }

    //            foreach (Row row in pallete.Rows)
    //            {
    //                var rowViewModel = _viewModelFactory.Create<RowViewModel, Row>(row);

    //                await rowViewModel.CreateAsync(pallete.Width);
    //                RowViewModels.Add(rowViewModel);
    //            }

    //            await _mediator.Publish(new MessageNotification(puzzleId, state, specialMessage));
    //        }
    //    }
    //}
}
