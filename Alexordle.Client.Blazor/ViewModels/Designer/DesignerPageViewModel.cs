using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Models;
using Alexordle.Client.Application.Services;
using Alexordle.Client.Blazor.Factories;
using Alexordle.Client.Blazor.Models;
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

        PalleteViewModel = palleteViewModel;

        AnswerInputViewModels = [];
        ClueInputViewModels = [];
    }

    private bool IsChanged { get; set; }
    private bool IsDelayed { get; set; }

    [ObservableProperty]
    private bool _isLoading;

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
        IsLoading = true;
        IsChanged = true;

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
    private void Loaded()
    {
        WidthInputViewModel = _inputViewModelFactory.CreateInputViewModel<WidthRuleSet>("Width");
        MaxGuessesInputViewModel = _inputViewModelFactory.CreateInputViewModel<MaxGuessesRuleSet>("Maximum Guesses");

        WidthInputViewModel.Text = "5";
        MaxGuessesInputViewModel.Text = "8";
    }

    [RelayCommand]
    private void AddAnswer()
    {
        var answerListInputViewModel = _inputViewModelFactory.CreateListInputViewModel<AnswerRuleSet>();

        AnswerInputViewModels.Add(answerListInputViewModel);
    }

    [RelayCommand]
    private void AddClue()
    {
        var clueListInputViewModel = _inputViewModelFactory.CreateListInputViewModel<ClueRuleSet>();

        ClueInputViewModels.Add(clueListInputViewModel);
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
    private async Task IsSpellCheckingChangedAsync()
    {
        await GeneratePuzzleAsync();
    }

    private async Task<string> SerializePuzzleAsync()
    {
        DesignValues designValues = GetDesignValues();

        Puzzle puzzle = await _puzzleService.StartPuzzleAsync(designValues.Width, designValues.MaxGuesses, designValues.IsSpellChecking, designValues.Clues, designValues.Answers);

        return await _serializationService.SerializePuzzleAsync(puzzle.Id);
    }

    private async Task GeneratePuzzleAsync()
    {
        IsSuccessfullyGenerated = false;

        try
        {
            await _puzzleService.DeletePuzzlesAsync();

            DesignValues designValues = GetDesignValues();

            PuzzleDesign design = await _puzzleService.CreateDesignAsync(designValues.Width, designValues.MaxGuesses, designValues.IsSpellChecking, designValues.Clues, designValues.Answers);

            foreach (string answer in designValues.Answers)
            {
                for (int index = 0; index < designValues.Width; index++)
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

                    await _hunchService.AppendCharacterToHunchAsync(design.PuzzleId, character);
                }

                await _hunchService.SubmitDesignHunchAsync(design.PuzzleId);
            }

            _designerWidthValidation.CurrentWidth = designValues.Width;
            _designerAnswersCountValidation.CurrentAnswersCount = AnswerInputViewModels.Count;

            HasDuplicateAnswerCharacter = design.HasDuplicateAnswerCharacter;

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

            await _mediator.Publish(new PalleteUpdateNotification(design.PuzzleId));

            IsSuccessfullyGenerated = true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to generate the puzzle.");
        }
    }

    private DesignValues GetDesignValues()
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

        if (!int.TryParse(MaxGuessesInputViewModel?.Text, out int maxGuesses))
        {
            maxGuesses = Math.Max(8 - answers.Count, 1);
        }
        maxGuesses = Math.Min(maxGuesses, Puzzle.VALIDATION_MAXGUESSES_MAXIMUM);

        var clues = new List<string>();
        foreach (ListInputViewModel clueInputViewModel in ClueInputViewModels)
        {
            if (clueInputViewModel.Text is not null)
            {
                clues.Add(clueInputViewModel.Text.PadRight(width, ' '));
            }
        }

        return new DesignValues
        {
            Width = width,
            MaxGuesses = maxGuesses,
            Clues = clues,
            Answers = answers,
            IsSpellChecking = IsSpellChecking,
        };
    }
}
