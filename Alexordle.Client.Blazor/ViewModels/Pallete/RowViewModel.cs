using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Database.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Lexicom.Mvvm;
using System.Collections.ObjectModel;

namespace Alexordle.Client.Blazor.ViewModels.Pallete;
public partial class RowViewModel : ObservableObject
{
    private readonly IViewModelFactory _viewModelFactory;

    public RowViewModel(IViewModelFactory viewModelFactory)
    {
        _viewModelFactory = viewModelFactory;

        CellViewModels = [];
    }

    [ObservableProperty]
    private ObservableCollection<CellViewModel> _cellViewModels;

    [ObservableProperty]
    private string? _annotation;

    public void Set(int width, IEnumerable<Clue> clues)
    {
        CellViewModels.Clear();

        foreach (Clue clue in clues)
        {
            Create(width, clue.InvariantCharacter, clue.Hint, isHunch: false);
        }

        //Annotation = Row.Annotation switch
        //{
        //    Annotations.Huh => "Huh?",
        //    Annotations.Bonus => "Bonus!",
        //    _ => string.Empty,
        //};
    }

    public void Set(int width, IEnumerable<Guess> guesses)
    {
        CellViewModels.Clear();

        foreach (Guess guess in guesses)
        {
            Create(width, guess.InvariantCharacter, guess.Hint, isHunch: false);
        }
    }

    public void Set(int width, IEnumerable<Hunch> hunches)
    {
        CellViewModels.Clear();

        var hunchesList = hunches.ToList();
        for (int count = 0; count < width; count++)
        {
            Hunch? hunch = null;
            if (count < hunchesList.Count)
            {
                hunch = hunchesList[count];
            }

            Create(width, hunch?.InvariantCharacter, hunch?.Hint ?? Hints.None, isHunch: true);
        }
    }

    private void Create(int width, char? invariantCharacter, Hints hint, bool isHunch)
    {
        CellViewModel cellViewModel = _viewModelFactory.Create<CellViewModel>();

        cellViewModel.Create(width, invariantCharacter, hint, isHunch);

        CellViewModels.Add(cellViewModel);
    }
}
