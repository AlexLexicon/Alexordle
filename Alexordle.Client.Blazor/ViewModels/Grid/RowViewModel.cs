using Alexordle.Client.Application.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Lexicom.Mvvm;
using System.Collections.ObjectModel;

namespace Alexordle.Client.Blazor.ViewModels.Grid;
public partial class RowViewModel : ObservableObject
{
    private readonly IViewModelFactory _viewModelFactory;

    public RowViewModel(
        Row row,
        IViewModelFactory viewModelFactory)
    {
        _viewModelFactory = viewModelFactory;

        Row = row;

        CellViewModels = [];
    }

    private Row Row { get; set; }

    [ObservableProperty]
    private ObservableCollection<CellViewModel> _cellViewModels;

    [ObservableProperty]
    private string? _annotation;

    public async Task CreateAsync()
    {
        Annotation = Row.Annotation switch
        {
            Annotations.Huh => "Huh?",
            Annotations.Bonus => "Bonus!",
            _ => string.Empty,
        };

        foreach (Cell cell in Row.Cells)
        {
            var cellViewModel = _viewModelFactory.Create<CellViewModel, Cell>(cell);

            await cellViewModel.CreateAsync();

            CellViewModels.Add(cellViewModel);
        }
    }
}
