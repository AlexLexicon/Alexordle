using Alexordle.Client.Application.Models;
using Alexordle.Client.Blazor.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using MediatR;

namespace Alexordle.Client.Blazor.ViewModels.Grid;
public partial class CellViewModel : ObservableObject
{
    private readonly IMediator _mediator;

    public CellViewModel(
        Cell cell, 
        IMediator mediator)
    {
        _mediator = mediator;

        Cell = cell;
    }

    private Cell Cell { get; }

    [ObservableProperty]
    private string? _text;

    [ObservableProperty]
    private Highlights _highlight;

    [ObservableProperty]
    private int _width;

    public async Task CreateAsync(int width)
    {
        Width = width;

        Text = Cell.IsEmpty ? string.Empty : Cell.Text;
        Highlight = Cell.IsObscured ? Highlights.CommittedIncorrect : Cell.Highlight;

        await _mediator.Publish(new CharacterHighlightNotification(Cell.InvariantCharacter, Cell.Highlight, Cell.IsFinished));
    }
}
