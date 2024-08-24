using Alexordle.Client.Application.Models;
using Alexordle.Client.Application.Services;
using Alexordle.Client.Blazor.Models;
using Alexordle.Client.Blazor.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lexicom.Concentrate.Blazor.WebAssembly.Amenities.Notifications;
using Lexicom.Mvvm;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace Alexordle.Client.Blazor.ViewModels.Grid;
public partial class PalleteViewModel : ObservableObject, INotificationHandler<GeneratePalleteNotification>, INotificationHandler<PeriodicTickNotification>
{
    private readonly ILogger<PalleteViewModel> _logger;
    private readonly IMediator _mediator;
    private readonly IViewModelFactory _viewModelFactory;
    private readonly IPalleteService _palleteService;
    private readonly IStateService _stateService;

    public PalleteViewModel(
        ILogger<PalleteViewModel> logger,
        IViewModelFactory viewModelFactory,
        IPalleteService palleteService,
        IStateService stateService,
        IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
        _viewModelFactory = viewModelFactory;
        _palleteService = palleteService;
        _stateService = stateService;

        ClueRowViewModels = [];
        RowViewModels = [];
    }

    public bool IsReload { get; set; }
    public bool IsLoadable { get; set; }
    private bool IsGenerating { get; set; }
    private long LastGeneratedUtcTick { get; set; }
    private GeneratePalleteNotification? GeneratePalleteNotification { get; set; }

    [ObservableProperty]
    private ObservableCollection<RowViewModel> _clueRowViewModels;

    [ObservableProperty]
    private ObservableCollection<RowViewModel> _rowViewModels;

    [ObservableProperty]
    public bool _isLoading;

    [ObservableProperty]
    private bool _invalid;

    [ObservableProperty]
    private string? _columns;

    public Task Handle(GeneratePalleteNotification notification, CancellationToken cancellationToken)
    {
        if (notification.InitiatedUtcTick > LastGeneratedUtcTick)
        {
            if (IsLoadable)
            {
                IsLoading = true;
            }

            GeneratePalleteNotification = notification;
        }

        return Task.CompletedTask;
    }

    public async Task Handle(PeriodicTickNotification notification, CancellationToken cancellationToken)
    {
        if (GeneratePalleteNotification is not null && !IsGenerating)
        {
            IsReload = false;
            IsGenerating = true;

            await GenerateAsync(GeneratePalleteNotification.PuzzleId, GeneratePalleteNotification.State, GeneratePalleteNotification.SpecialMessage, GeneratePalleteNotification.IsDesigner);

            LastGeneratedUtcTick = notification.UtcNow.Ticks;
            GeneratePalleteNotification = null;

            IsGenerating = false;
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Loaded()
    {
        IsLoading = true;
    }

    private async Task GenerateAsync(Guid puzzleId, State? state, SpecialMessages? specialMessage, bool isDesigner)
    {
        if (!IsReload)
        {
            ClueRowViewModels.Clear();
            RowViewModels.Clear();
            state ??= await _stateService.GetStateAsync(puzzleId);
            Pallete pallete = await _palleteService.GeneratePalleteAsync(puzzleId, state, isDesigner);

            Invalid = pallete.Width is <= 0;
            if (!Invalid)
            {
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

                await _mediator.Publish(new MessageNotification(puzzleId, state, specialMessage));
            }
        }
    }
}
