using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Models;
using Alexordle.Client.Application.Services;
using Alexordle.Client.Blazor.Notifications;
using Alexordle.Client.Blazor.Validations;
using Alexordle.Client.Blazor.Validations.RuleSets;
using Alexordle.Client.Blazor.ViewModels.Pallete;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lexicom.Concentrate.Blazor.WebAssembly.Amenities.Notifications;
using Lexicom.Concentrate.Blazor.WebAssembly.Amenities.Services;
using Lexicom.DependencyInjection.Primitives;
using Lexicom.Mvvm;
using Lexicom.Validation;
using MediatR;
using System.Collections.ObjectModel;

namespace Alexordle.Client.Blazor.ViewModels.Designer;
public partial class DesignerPageViewModel : ObservableObject//, INotificationHandler<RefreshDesignerNotification>, INotificationHandler<RemoveClueInputNotification>, INotificationHandler<RemoveAnswerInputNotification>, INotificationHandler<PeriodicTickNotification>
{
    //private readonly IMediator _mediator;
    //private readonly IViewModelFactory _viewModelFactory;
    //private readonly IPuzzleService _puzzleService;
    //private readonly IClipboardService _clipboardService;
    //private readonly IGameService _gameService;
    //private readonly IUrlService _urlService;
    //private readonly INavigationService _navigationService;
    //private readonly ITimeProvider _timeProvider;
    //private readonly DesignerAnswersCountValidation _designerAnswersCountValidation;
    //private readonly DesignerWidthValidation _designerWidthValidation;

    //public DesignerPageViewModel(
    //    IMediator mediator,
    //    IViewModelFactory viewModelFactory,
    //    IPuzzleService puzzleService,
    //    IClipboardService clipboardService,
    //    IGameService gameService,
    //    IUrlService urlService,
    //    INavigationService navigationService,
    //    ITimeProvider timeProvider,
    //    DesignerAnswersCountValidation designerAnswersCountValidation,
    //    DesignerWidthValidation designerWidthValidation,
    //    PalleteViewModel palleteViewModel,
    //    IRuleSetValidator<WidthRuleSet, string?> widthTextRuleSetValidator,
    //    IRuleSetValidator<MaximumGuessesRuleSet, string?> maximumGuessesTextRuleSetValidator)
    //{
    //    _mediator = mediator;
    //    _viewModelFactory = viewModelFactory;
    //    _puzzleService = puzzleService;
    //    _clipboardService = clipboardService;
    //    _gameService = gameService;
    //    _urlService = urlService;
    //    _navigationService = navigationService;
    //    _designerAnswersCountValidation = designerAnswersCountValidation;
    //    _designerWidthValidation = designerWidthValidation;
    //    _timeProvider = timeProvider;

    //    PalleteViewModel = palleteViewModel;
    //    WidthTextRuleSetValidator = widthTextRuleSetValidator;
    //    MaximumGuessesTextRuleSetValidator = maximumGuessesTextRuleSetValidator;

    //    ClueInputViewModels = [];
    //    AnswerInputViewModels = [];
    //    DesignerState = new State
    //    {
    //        TotalAnswers = 0,
    //        TotalGuesses = 0,
    //        MaximumGuesses = 0,
    //        RemainingAnswers = 0,
    //        RemainingGuesses = 0,
    //        IsBonus = false,
    //        IsDefeat = false,
    //        IsVictory = false,
    //        CorrectGuessIds = new HashSet<Guid>(),
    //    };
    //}

    //private Guid PuzzleId { get; set; }
    //private string? GameUrl { get; set; }
    //private State DesignerState { get; }
    //private bool IsChanging { get; set; }
    //private long CurrentChagedUtcTick { get; set; }
    //private long LastChangedUtcTick { get; set; }


    //[ObservableProperty]
    //private bool _isSpellChecking;

    //[ObservableProperty]
    //private bool _isSpellCheckingValid;

    //[ObservableProperty]
    //private string? _widthText;

    //[ObservableProperty]
    //private IRuleSetValidator<WidthRuleSet, string?> _widthTextRuleSetValidator;

    //[ObservableProperty]
    //private string? _maximumGuessesText;

    //[ObservableProperty]
    //private IRuleSetValidator<MaximumGuessesRuleSet, string?> _maximumGuessesTextRuleSetValidator;

    //[ObservableProperty]
    //private ObservableCollection<ClueInputViewModel> _clueInputViewModels;

    //[ObservableProperty]
    //private ObservableCollection<AnswerInputViewModel> _answerInputViewModels;

    //[ObservableProperty]
    //private PalleteViewModel _palleteViewModel;

    //[ObservableProperty]
    //private bool _hasAnswers;

    //[ObservableProperty]
    //private bool _isPlayable;

    //public async Task RedirectAsync(string puzzleCode)
    //{
    //    string url = await _urlService.CreatePuzzleUrlAsync(puzzleCode);

    //    await _navigationService.NavigateToUrlAsync(url);
    //}

    //public async Task Handle(RefreshDesignerNotification notification, CancellationToken cancellationToken)
    //{
    //    await RefreshDesignerAsync();
    //}

    //public async Task Handle(RemoveClueInputNotification notification, CancellationToken cancellationToken)
    //{
    //    ClueInputViewModels.Remove(notification.ClueInputViewModel);

    //    await RefreshDesignerAsync();
    //}

    //public async Task Handle(RemoveAnswerInputNotification notification, CancellationToken cancellationToken)
    //{
    //    AnswerInputViewModels.Remove(notification.AnswerInputViewModel);

    //    SetHasAnswers();

    //    await RefreshDesignerAsync();
    //}

    //public async Task Handle(PeriodicTickNotification notification, CancellationToken cancellationToken)
    //{
    //    if (!IsChanging && CurrentChagedUtcTick > LastChangedUtcTick)
    //    {
    //        IsChanging = true;

    //        await UpdateWidthAsync();
    //        await UpdateMaximumGuessesAsync();

    //        await RefreshDesignerAsync();

    //        LastChangedUtcTick = notification.UtcNow.Ticks;

    //        IsChanging = false;
    //    }
    //}

    //[RelayCommand]
    //private Task WidthTextChangedAsync()
    //{
    //    CurrentChagedUtcTick = _timeProvider.GetUtcNow().UtcTicks;

    //    return Task.CompletedTask;
    //}

    //[RelayCommand]
    //private Task MaximumGuessesTextChangedAsync()
    //{
    //    CurrentChagedUtcTick = _timeProvider.GetUtcNow().UtcTicks;

    //    return Task.CompletedTask;
    //}

    //[RelayCommand]
    //private async Task IsSpellCheckingChangedAsync()
    //{
    //    await UpdateIsSpellCheckingAsync();

    //    await RefreshDesignerAsync();
    //}

    //[RelayCommand]
    //private async Task LoadedAsync()
    //{
    //    IsPlayable = false;

    //    await _puzzleService.DeleteAllPuzzlesAsync();

    //    Puzzle puzzle = await _puzzleService.CreatePuzzleAsync();

    //    PuzzleId = puzzle.Id;

    //    IsSpellChecking = true;
    //    WidthText = "5";
    //    MaximumGuessesText = "8";

    //    await ValidateAsync();

    //    //default xordle setup
    //    await UpdateIsSpellCheckingAsync();
    //    await UpdateWidthAsync();
    //    await UpdateMaximumGuessesAsync();
    //    await CreateClueInputAsync("words");
    //    await CreateAnswerInputAsync("games");

    //    await RefreshDesignerAsync();
    //}

    //[RelayCommand]
    //private async Task AddClueInputAsync()
    //{
    //    await CreateClueInputAsync(string.Empty);

    //    await RefreshDesignerAsync();
    //}

    //[RelayCommand]
    //private async Task AddAnswerInputAsync()
    //{
    //    await CreateAnswerInputAsync(string.Empty);

    //    await RefreshDesignerAsync();
    //}

    //[RelayCommand]
    //private async Task CopyGameUrlAsync()
    //{
    //    if (IsPlayable && !string.IsNullOrWhiteSpace(GameUrl))
    //    {
    //        await _clipboardService.WriteAsync(GameUrl);
    //    }
    //}

    //[RelayCommand]
    //private async Task PlayGameAsync()
    //{
    //    if (IsPlayable && !string.IsNullOrWhiteSpace(GameUrl))
    //    {
    //        await _navigationService.NavigateToUrlAsync(GameUrl);
    //    }
    //}

    //private async Task CreateClueInputAsync(string initalClueText)
    //{
    //    var clueInputViewModel = _viewModelFactory.Create<ClueInputViewModel, Guid>(PuzzleId);

    //    ClueInputViewModels.Add(clueInputViewModel);

    //    await clueInputViewModel.CreateAsync(initalClueText);
    //}

    //private async Task CreateAnswerInputAsync(string initalAnswerText)
    //{
    //    var answerInputViewModel = _viewModelFactory.Create<AnswerInputViewModel, Guid>(PuzzleId);

    //    AnswerInputViewModels.Add(answerInputViewModel);

    //    SetHasAnswers();

    //    await answerInputViewModel.CreateAsync(initalAnswerText);
    //}

    //private async Task UpdateIsSpellCheckingAsync()
    //{
    //    await _puzzleService.SetIsSpellCheckingAsync(PuzzleId, IsSpellChecking);

    //    await ValidateAsync();
    //}

    //private async Task UpdateWidthAsync()
    //{
    //    int width = GetWidth();

    //    _designerWidthValidation.CurrentWidth = width;

    //    await _puzzleService.SetWidthAsync(PuzzleId, width);

    //    await _mediator.Publish(new WidthChangedNotification());

    //    await ValidateAsync();
    //}

    //private async Task UpdateMaximumGuessesAsync()
    //{
    //    int maximumGuesses = GetMaximumGuesses();

    //    _designerAnswersCountValidation.CurrentAnswersCount = AnswerInputViewModels.Count;

    //    await _puzzleService.SetMaximumGuessesAsync(PuzzleId, maximumGuesses);

    //    await ValidateAsync();
    //}

    //private async Task ValidateAsync()
    //{
    //    await WidthTextRuleSetValidator.ValidateAsync(WidthText);
    //    await MaximumGuessesTextRuleSetValidator.ValidateAsync(MaximumGuessesText);
    //    IsSpellCheckingValid = !IsSpellChecking || IsSpellChecking && GetWidth() is 5;
    //}

    //private async Task RefreshDesignerAsync()
    //{
    //    IsPlayable = GetIsPlayable();
    //    if (IsPlayable)
    //    {
    //        string code = await _gameService.EncodePuzzleAsync(PuzzleId);

    //        GameUrl = await _urlService.CreatePuzzleUrlAsync(code);
    //    }

    //    long tick = _timeProvider.GetUtcNow().Ticks;

    //    await _mediator.Publish(new GeneratePalleteNotification(tick, PuzzleId, DesignerState, IsDesigner: true));
    //}

    //private bool GetIsPlayable()
    //{
    //    if (MaximumGuessesTextRuleSetValidator.IsValid && WidthTextRuleSetValidator.IsValid && IsSpellCheckingValid && HasAnswers)
    //    {
    //        foreach (ClueInputViewModel clueInputViewModel in ClueInputViewModels)
    //        {
    //            if (!clueInputViewModel.TextRuleSetValidator.IsValid)
    //            {
    //                return false;
    //            }
    //        }

    //        foreach (AnswerInputViewModel answerInputViewModel in AnswerInputViewModels)
    //        {
    //            if (!answerInputViewModel.TextRuleSetValidator.IsValid)
    //            {
    //                return false;
    //            }
    //        }

    //        return true;
    //    }

    //    return false;
    //}

    //private void SetHasAnswers()
    //{
    //    HasAnswers = AnswerInputViewModels.Count is > 0;
    //}

    //private int GetMaximumGuesses()
    //{
    //    if (MaximumGuessesTextRuleSetValidator.IsValid && int.TryParse(MaximumGuessesText, out int maximumGuesses))
    //    {
    //        return maximumGuesses;
    //    }

    //    return 0;
    //}

    //private int GetWidth()
    //{
    //    if (WidthTextRuleSetValidator.IsValid && int.TryParse(WidthText, out int width))
    //    {
    //        return width;
    //    }

    //    return 0;
    //}
}
