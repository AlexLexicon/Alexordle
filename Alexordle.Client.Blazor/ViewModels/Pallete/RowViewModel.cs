using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Database.Models;
using Alexordle.Client.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Lexicom.Mvvm;
using System.Collections.ObjectModel;

namespace Alexordle.Client.Blazor.ViewModels.Pallete;
public partial class RowViewModel : ObservableObject
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly IGuessService _guessService;
    private readonly IHunchService _hunchService;

    public RowViewModel(
        IViewModelFactory viewModelFactory,
        IGuessService guessService,
        IHunchService hunchService)
    {
        _viewModelFactory = viewModelFactory;
        _guessService = guessService;
        _hunchService = hunchService;

        CellViewModels = [];
    }

    [ObservableProperty]
    private ObservableCollection<CellViewModel> _cellViewModels;

    [ObservableProperty]
    private bool _isBonus;

    [ObservableProperty]
    private bool _isHuh;

    [ObservableProperty]
    private bool _isInfinite;

    public void Set(int width, IEnumerable<ClueCharacter> clues)
    {
        CellViewModels.Clear();

        foreach (ClueCharacter clue in clues)
        {
            Create(width, clue.InvariantCharacter, clue.Hint, isHunch: false);
        }
    }

    public async void Set(int width, IEnumerable<GuessCharacter> guesses)
    {
        CellViewModels.Clear();

        Guess? guess = null;
        foreach (GuessCharacter guessCharacter in guesses)
        {
            guess ??= await _guessService.GetGuessAsync(guessCharacter.GuessId);

            Create(width, guessCharacter.InvariantCharacter, guessCharacter.Hint, isHunch: false);
        }

        if (guess is not null)
        {
            IsInfinite = false;
            IsBonus = guess.IsBonus;
            IsHuh = guess.IsHuh;
        }
    }

    public async void Set(Guid puzzleId, int width, IEnumerable<HunchCharacter> hunches)
    {
        CellViewModels.Clear();

        Hunch? hunch = null;

        var hunchesList = hunches.ToList();
        for (int count = 0; count < width; count++)
        {
            hunch ??= await _hunchService.GetHunchAsync(puzzleId);

            HunchCharacter? hunchCharacter = null;
            if (count < hunchesList.Count)
            {
                hunchCharacter = hunchesList[count];
            }

            Create(width, hunchCharacter?.InvariantCharacter, hunchCharacter?.Hint ?? Hints.None, isHunch: true);
        }

        if (hunch is not null)
        {
            IsInfinite = hunch.IsInfinite;
            IsBonus = hunch.IsBonus;
        }
    }

    public void Set(int width)
    {
        CellViewModels.Clear();

        for (int count = 0; count < width; count++)
        {
            Create(width, null, Hints.None, isHunch: false);
        }
    }

    private void Create(int width, char? invariantCharacter, Hints hint, bool isHunch)
    {
        CellViewModel cellViewModel = _viewModelFactory.Create<CellViewModel>();

        cellViewModel.Create(width, invariantCharacter, hint, isHunch);

        CellViewModels.Add(cellViewModel);
    }
}
