using Alexordle.Client.Blazor.Notifications;
using Alexordle.Client.Blazor.Validations.RuleSets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lexicom.Concentrate.Blazor.WebAssembly.Amenities.Notifications;
using Lexicom.DependencyInjection.Primitives;
using Lexicom.Validation;
using MediatR;

namespace Alexordle.Client.Blazor.ViewModels.Designer;
public abstract partial class AbstractInputViewModel : ObservableObject, INotificationHandler<WidthChangedNotification>, IDisposable, INotificationHandler<PeriodicTickNotification>
{
    private readonly IMediator _mediator;
    private readonly ITimeProvider _timeProvider;

    public AbstractInputViewModel(
        Guid puzzleId,
        IMediator mediator,
        ITimeProvider timeProvider,
        IRuleSetValidator<AnswerInputRuleSet, string?> answerTextRuleSetValidator)
    {
        _mediator = mediator;
        _timeProvider = timeProvider;

        PuzzleId = puzzleId;

        TextRuleSetValidator = answerTextRuleSetValidator;
    }

    private bool IsDisposed { get; set; }
    private bool IsChanging { get; set; }
    private long CurrentChagedUtcTick { get; set; }
    private long LastChangedUtcTick { get; set; }

    protected Guid PuzzleId { get; }

    [ObservableProperty]
    private string? _text;

    [ObservableProperty]
    private IRuleSetValidator<AnswerInputRuleSet, string?> _textRuleSetValidator;

    public virtual async Task CreateAsync(string initalText)
    {
        Text = initalText;

        await TextRuleSetValidator.ValidateAsync(Text);

        await RefreshDesignerAsync();
    }

    public async Task Handle(WidthChangedNotification notification, CancellationToken cancellationToken)
    {
        //todo removed the dispose logic cause i dont think its needed
        if (!IsDisposed)
        {
            await RefreshDesignerAsync();
        }
    }

    public async Task Handle(PeriodicTickNotification notification, CancellationToken cancellationToken)
    {
        if (!IsChanging && CurrentChagedUtcTick > LastChangedUtcTick)
        {
            IsChanging = true;

            await RefreshDesignerAsync();

            await _mediator.Publish(new RefreshDesignerNotification(), cancellationToken);

            LastChangedUtcTick = notification.UtcNow.Ticks;

            IsChanging = false;
        }
    }

    [RelayCommand]
    protected virtual Task TextChangedAsync()
    {
        CurrentChagedUtcTick = _timeProvider.GetUtcNow().UtcTicks;

        return Task.CompletedTask;
    }

    [RelayCommand]
    protected virtual async Task RemoveAsync()
    {
        Dispose();

        await DeleteAsync();

        await _mediator.Publish(GetRemoveInputNotification());
    }

    protected abstract INotification GetRemoveInputNotification();

    protected virtual async Task RefreshDesignerAsync()
    {
        if (Text is not null)
        {
            await DeleteAsync();
        }

        await TextRuleSetValidator.ValidateAsync(Text);
    }

    protected abstract Task DeleteAsync();

    public void Dispose()
    {
        IsDisposed = true;
    }
}
