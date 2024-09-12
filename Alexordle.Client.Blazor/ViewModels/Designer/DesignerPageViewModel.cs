using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Models;
using Alexordle.Client.Application.Services;
using Alexordle.Client.Blazor.Factories;
using Alexordle.Client.Blazor.Notifications;
using Alexordle.Client.Blazor.Validations;
using Alexordle.Client.Blazor.Validations.RuleSets;
using Alexordle.Client.Blazor.ViewModels.Pallete;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lexicom.Concentrate.Blazor.WebAssembly.Amenities.Notifications;
using Lexicom.Concentrate.Blazor.WebAssembly.Amenities.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace Alexordle.Client.Blazor.ViewModels.Designer;
public partial class DesignerPageViewModel : ObservableObject, INotificationHandler<RemoveListInputViewModelNotification>, INotificationHandler<DesignChangedNotification>, INotificationHandler<PeriodicTickNotification>
{
    private readonly ILogger<DesignerPageViewModel> _logger;
    private readonly IMediator _mediator;
    private readonly IInputViewModelFactory _inputViewModelFactory;
    private readonly INavigationService _navigationService;
    private readonly IUrlService _urlService;
    private readonly IPuzzleService _puzzleService;
    private readonly IHunchService _hunchService;
    private readonly ISerializationService _serializationService;
    private readonly IClipboardService _clipboardService;
    private readonly IPersistenceService _persistenceService;
    private readonly DesignerWidthValidation _designerWidthValidation;
    private readonly DesignerAnswersCountValidation _designerAnswersCountValidation;

    public DesignerPageViewModel(
        ILogger<DesignerPageViewModel> logger,
        IMediator mediator,
        IInputViewModelFactory inputViewModelFactory,
        INavigationService navigationService,
        IUrlService urlService,
        IPuzzleService puzzleService,
        IHunchService hunchService,
        ISerializationService serializationService,
        IClipboardService clipboardService,
        IPersistenceService persistenceService,
        PalleteViewModel palleteViewModel,
        DesignerWidthValidation designerWidthValidation,
        DesignerAnswersCountValidation designerAnswersCountValidation)
    {
        _logger = logger;
        _mediator = mediator;
        _inputViewModelFactory = inputViewModelFactory;
        _navigationService = navigationService;
        _urlService = urlService;
        _puzzleService = puzzleService;
        _hunchService = hunchService;
        _serializationService = serializationService;
        _designerWidthValidation = designerWidthValidation;
        _designerAnswersCountValidation = designerAnswersCountValidation;
        _clipboardService = clipboardService;
        _persistenceService = persistenceService;

        PalleteViewModel = palleteViewModel;

        AnswerInputViewModels = [];
        ClueInputViewModels = [];
    }

    private bool IsChanged { get; set; }
    private bool IsDelayed { get; set; }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isInfiniteGuesses;

    [ObservableProperty]
    private bool _isSpellChecking;

    [ObservableProperty]
    private bool _isSuccessfullyGenerated;

    [ObservableProperty]
    private bool _isEnoughAnswers;

    [ObservableProperty]
    private bool _hasDuplicateAnswerCharacter;

    [ObservableProperty]
    private bool _isValid;

    [ObservableProperty]
    private PalleteViewModel _palleteViewModel;

    [ObservableProperty]
    private InputViewModel? _widthInputViewModel;

    [ObservableProperty]
    private InputViewModel? _maxGuessesInputViewModel;

    [ObservableProperty]
    private ObservableCollection<ListInputViewModel> _answerInputViewModels;

    [ObservableProperty]
    private ObservableCollection<ListInputViewModel> _clueInputViewModels;

    public async Task RedirectAsync(string serializedPuzzle)
    {
        string puzzleUrl = await _urlService.CreatePuzzleUrlAsync(serializedPuzzle);

        await _navigationService.NavigateToUrlAsync(puzzleUrl);
    }

    public Task Handle(RemoveListInputViewModelNotification notification, CancellationToken cancellationToken)
    {
        AnswerInputViewModels.Remove(notification.ListInputViewModel);
        ClueInputViewModels.Remove(notification.ListInputViewModel);

        return Task.CompletedTask;
    }

    public Task Handle(DesignChangedNotification notification, CancellationToken cancellationToken)
    {
        DesignChanged();

        return Task.CompletedTask;
    }

    public async Task Handle(PeriodicTickNotification notification, CancellationToken cancellationToken)
    {
        if (IsDelayed && !IsChanged)
        {
            IsDelayed = false;

            await GeneratePuzzleAsync();

            IsLoading = false;
        }

        if (IsChanged)
        {
            IsDelayed = true;
            IsChanged = false;
        }
    }

    [RelayCommand]
    private async Task LoadedAsync()
    {
        WidthInputViewModel = _inputViewModelFactory.CreateInputViewModel<WidthRuleSet>("Width");
        MaxGuessesInputViewModel = _inputViewModelFactory.CreateInputViewModel<MaxGuessesRuleSet>("Maximum Guesses");

        Design? design = await _persistenceService.LoadDesignAsync();
        await ResetDesignAsync(design);
    }

    [RelayCommand]
    private void AddAnswer()
    {
        CreateAnswer();
    }

    [RelayCommand]
    private void AddClue()
    {
        CreateClue();
    }

    [RelayCommand]
    private async Task ResetAsync()
    {
        await ResetDesignAsync(null);

        DesignChanged();
    }

    [RelayCommand]
    private async Task ShareAsync()
    {
        try
        {
            string serializedPuzzle = await SerializePuzzleAsync();

            string puzzleUrl = await _urlService.CreatePuzzleUrlAsync(serializedPuzzle);

            await _clipboardService.WriteAsync(puzzleUrl);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to share the puzzle.");
        }
    }

    [RelayCommand]
    private async Task PlayAsync()
    {
        try
        {
            string serializedPuzzle = await SerializePuzzleAsync();

            await RedirectAsync(serializedPuzzle);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to play the puzzle.");
        }
    }

    [RelayCommand]
    private void IsSpellCheckingChanged()
    {
        DesignChanged();
    }

    [RelayCommand]
    private void IsInfiniteGuessesChanged()
    {
        if (MaxGuessesInputViewModel is not null)
        {
            MaxGuessesInputViewModel.IsInputVisible = !IsInfiniteGuesses;
        }

        DesignChanged();
    }

    private async Task<string> SerializePuzzleAsync()
    {
        Design design = CreateDesign();

        Puzzle puzzle = await _puzzleService.StartPuzzleAsync(design.Width, design.MaxGuesses, design.IsSpellChecking, design.Clues, design.Answers);

        return await _serializationService.SerializePuzzleAsync(puzzle.Id);
    }

    private async Task GeneratePuzzleAsync()
    {
        IsSuccessfullyGenerated = false;

        try
        {
            await _puzzleService.DeletePuzzlesAsync();

            Design design = CreateDesign();

            DesignResult result = await _puzzleService.CreateDesignAsync(design.Width, design.MaxGuesses, design.IsSpellChecking, design.Clues, design.Answers);

            foreach (string answer in design.Answers)
            {
                for (int index = 0; index < design.Width; index++)
                {
                    char character = ' ';
                    if (index < answer.Length)
                    {
                        char answerCharacter = char.ToUpperInvariant(answer[index]);

                        if (AnswerCharacter.VALIDATION_CHARACTERS_SUPPORTED.Contains(answerCharacter))
                        {
                            character = answerCharacter;
                        }
                    }

                    await _hunchService.AppendCharacterToHunchAsync(result.PuzzleId, character);
                }

                await _hunchService.SubmitDesignHunchAsync(result.PuzzleId);
            }

            await CheckAndValidateAsync(design.Width, result.HasDuplicateAnswerCharacter);

            await _persistenceService.SaveDesignAsync(result.PuzzleId);

            await _mediator.Publish(new PalleteUpdateNotification(result.PuzzleId));

            IsSuccessfullyGenerated = true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to generate the puzzle.");
        }
    }

    private Design CreateDesign()
    {
        if (!int.TryParse(WidthInputViewModel?.Text, out int width))
        {
            width = 0;
        }

        var answers = new List<string>();
        foreach (ListInputViewModel answerInputViewModel in AnswerInputViewModels)
        {
            string? answer = answerInputViewModel.Text?.PadRight(width, ' ');
            if (!string.IsNullOrWhiteSpace(answer))
            {
                answers.Add(answer);
            }
        }
        IsEnoughAnswers = answers.Count > 0;

        if (answers.Count is <= 0)
        {
            answers.Add(new string(' ', width));
        }

        int? nullableMaxGuesses = null;
        if (!IsInfiniteGuesses)
        {
            if (!int.TryParse(MaxGuessesInputViewModel?.Text, out int maxGuesses))
            {
                maxGuesses = Math.Max(8 - answers.Count, 1);
            }

            nullableMaxGuesses = Math.Min(maxGuesses, Puzzle.VALIDATION_MAXGUESSES_MAXIMUM);
        }

        var clues = new List<string>();
        foreach (ListInputViewModel clueInputViewModel in ClueInputViewModels)
        {
            if (clueInputViewModel.Text is not null)
            {
                clues.Add(clueInputViewModel.Text.PadRight(width, ' '));
            }
        }

        return new Design
        {
            Width = width,
            MaxGuesses = nullableMaxGuesses,
            IsSpellChecking = IsSpellChecking,
            Clues = clues,
            Answers = answers,
        };
    }

    private async Task ResetDesignAsync(Design? design)
    {
        int width = design?.Width ?? 5;

        if (WidthInputViewModel is not null)
        {
            WidthInputViewModel.Text = width.ToString();
        }

        if (MaxGuessesInputViewModel is not null)
        {
            MaxGuessesInputViewModel.Text = design?.MaxGuesses.ToString() ?? "8";
        }

        IsSpellChecking = design?.IsSpellChecking ?? true;

        ClueInputViewModels.Clear();
        AnswerInputViewModels.Clear();

        if (design is not null)
        {
            foreach (string clue in design.Clues)
            {
                CreateClue(clue);
            }
            foreach (string answer in design.Answers)
            {
                CreateAnswer(answer);
            }
        }
        else
        {
            CreateClue("SPELL");
            CreateAnswer("GAMES");
        }

        IsEnoughAnswers = AnswerInputViewModels.Count is > 0;

        await CheckAndValidateAsync(width, false);
    }

    private async Task CheckAndValidateAsync(int width, bool hasDuplicateAnswerCharacter)
    {
        _designerWidthValidation.CurrentWidth = width;
        _designerAnswersCountValidation.CurrentAnswersCount = AnswerInputViewModels.Count;

        HasDuplicateAnswerCharacter = hasDuplicateAnswerCharacter;

        IsValid = IsEnoughAnswers && !HasDuplicateAnswerCharacter;
        if (WidthInputViewModel is not null)
        {
            bool isValid = await WidthInputViewModel.CheckAndValidateAsync();
            if (!isValid)
            {
                IsValid = false;
            }
        }
        if (MaxGuessesInputViewModel is not null)
        {
            bool isValid = await MaxGuessesInputViewModel.CheckAndValidateAsync();
            if (!isValid)
            {
                IsValid = false;
            }
        }
        foreach (ListInputViewModel answerInputViewModel in AnswerInputViewModels)
        {
            bool isValid = await answerInputViewModel.CheckAndValidateAsync();
            if (!isValid)
            {
                IsValid = false;
            }
        }
        foreach (ListInputViewModel clueInputViewModel in ClueInputViewModels)
        {
            bool isValid = await clueInputViewModel.CheckAndValidateAsync();
            if (!isValid)
            {
                IsValid = false;
            }
        }
    }

    private void DesignChanged()
    {
        IsLoading = true;
        IsChanged = true;
    }

    private void CreateAnswer(string? answer = null)
    {
        var answerListInputViewModel = _inputViewModelFactory.CreateListInputViewModel<AnswerRuleSet>();

        answerListInputViewModel.Text = answer;

        AnswerInputViewModels.Add(answerListInputViewModel);
    }

    private void CreateClue(string? clue = null)
    {
        var clueListInputViewModel = _inputViewModelFactory.CreateListInputViewModel<ClueRuleSet>();

        clueListInputViewModel.Text = clue;

        ClueInputViewModels.Add(clueListInputViewModel);
    }
}
