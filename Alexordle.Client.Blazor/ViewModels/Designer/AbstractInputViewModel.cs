using CommunityToolkit.Mvvm.ComponentModel;

namespace Alexordle.Client.Blazor.ViewModels.Designer;
public abstract partial class AbstractInputViewModel : ObservableObject//, INotificationHandler<WidthChangedNotification>, INotificationHandler<PeriodicTickNotification>
{
    //private readonly IMediator _mediator;
    //private readonly ITimeProvider _timeProvider;

    //public AbstractInputViewModel(
    //    Guid puzzleId,
    //    IMediator mediator,
    //    ITimeProvider timeProvider,
    //    IRuleSetValidator<AnswerInputRuleSet, string?> answerTextRuleSetValidator)
    //{
    //    _mediator = mediator;
    //    _timeProvider = timeProvider;

    //    PuzzleId = puzzleId;

    //    TextRuleSetValidator = answerTextRuleSetValidator;
    //}

    //private bool IsChanging { get; set; }
    //private long CurrentChagedUtcTick { get; set; }
    //private long LastChangedUtcTick { get; set; }
    //protected bool IsRemoved { get; set; }

    //protected Guid PuzzleId { get; }

    //[ObservableProperty]
    //private string? _text;

    //[ObservableProperty]
    //private IRuleSetValidator<AnswerInputRuleSet, string?> _textRuleSetValidator;

    //public virtual async Task CreateAsync(string initalText)
    //{
    //    Text = initalText;

    //    await TextRuleSetValidator.ValidateAsync(Text);

    //    await RefreshDesignerAsync();
    //}

    //public async Task Handle(WidthChangedNotification notification, CancellationToken cancellationToken)
    //{
    //    await RefreshDesignerAsync();
    //}

    //public async Task Handle(PeriodicTickNotification notification, CancellationToken cancellationToken)
    //{
    //    if (!IsChanging && CurrentChagedUtcTick > LastChangedUtcTick)
    //    {
    //        IsChanging = true;

    //        await RefreshDesignerAsync();

    //        await _mediator.Publish(new RefreshDesignerNotification(), cancellationToken);

    //        LastChangedUtcTick = notification.UtcNow.Ticks;

    //        IsChanging = false;
    //    }
    //}

    //[RelayCommand]
    //protected virtual Task TextChangedAsync()
    //{
    //    CurrentChagedUtcTick = _timeProvider.GetUtcNow().UtcTicks;

    //    return Task.CompletedTask;
    //}

    //[RelayCommand]
    //protected virtual async Task RemoveAsync()
    //{
    //    IsRemoved = true;

    //    await DeleteAsync();

    //    await _mediator.Publish(GetRemoveInputNotification());
    //}

    //protected abstract INotification GetRemoveInputNotification();

    //protected virtual async Task RefreshDesignerAsync()
    //{
    //    if (Text is not null)
    //    {
    //        await DeleteAsync();
    //    }

    //    await TextRuleSetValidator.ValidateAsync(Text);
    //}

    //protected abstract Task DeleteAsync();
}
