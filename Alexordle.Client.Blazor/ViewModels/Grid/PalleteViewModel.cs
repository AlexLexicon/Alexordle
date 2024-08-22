using Alexordle.Client.Application.Models;
using Alexordle.Client.Application.Services;
using Alexordle.Client.Blazor.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using Lexicom.Mvvm;
using MediatR;
using System.Collections.ObjectModel;

namespace Alexordle.Client.Blazor.ViewModels.Grid;
public partial class PalleteViewModel : ObservableObject, INotificationHandler<GeneratePalleteNotification>
{
    private readonly IMediator _mediator;
    private readonly IViewModelFactory _viewModelFactory;
    private readonly IPalleteService _palleteService;
    private readonly IStateService _stateService;

    public PalleteViewModel(
        IViewModelFactory viewModelFactory,
        IPalleteService palleteService,
        IStateService stateService,
        IMediator mediator)
    {
        _mediator = mediator;
        _viewModelFactory = viewModelFactory;
        _palleteService = palleteService;
        _stateService = stateService;

        ClueRowViewModels = [];
        RowViewModels = [];
    }

    [ObservableProperty]
    private ObservableCollection<RowViewModel> _clueRowViewModels;

    [ObservableProperty]
    private ObservableCollection<RowViewModel> _rowViewModels;

    [ObservableProperty]
    private string? _columns;

    public async Task Handle(GeneratePalleteNotification notification, CancellationToken cancellationToken)
    {
        ClueRowViewModels.Clear();
        RowViewModels.Clear();

        State? state = notification.State ?? await _stateService.GetStateAsync(notification.PuzzleId);

        Pallete pallete = await _palleteService.GeneratePalleteAsync(notification.PuzzleId, state, notification.IsDesigner);

        Columns = $"{pallete.Width + 2}";

        foreach (Row row in pallete.Clues)
        {
            var rowViewModel = _viewModelFactory.Create<RowViewModel, Row>(row);

            await rowViewModel.CreateAsync();

            ClueRowViewModels.Add(rowViewModel);
        }

        foreach (Row row in pallete.Rows)
        {
            var rowViewModel = _viewModelFactory.Create<RowViewModel, Row>(row);

            await rowViewModel.CreateAsync();

            RowViewModels.Add(rowViewModel);
        }

        await _mediator.Publish(new MessageNotification(notification.PuzzleId, state, notification.SpecialMessage), cancellationToken);
    }
}
