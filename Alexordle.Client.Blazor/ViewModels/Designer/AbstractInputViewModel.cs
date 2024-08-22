using Alexordle.Client.Blazor.Notifications;
using Alexordle.Client.Blazor.Validations.RuleSets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lexicom.Validation;
using MediatR;

namespace Alexordle.Client.Blazor.ViewModels.Designer;
public abstract partial class AbstractInputViewModel : ObservableObject, INotificationHandler<WidthChangedNotification>, IDisposable
{
    private readonly IMediator _mediator;

    public AbstractInputViewModel(
        Guid puzzleId,
        IMediator mediator,
        IRuleSetValidator<AnswerInputRuleSet, string?> answerTextRuleSetValidator)
    {
        _mediator = mediator;

        PuzzleId = puzzleId;

        TextRuleSetValidator = answerTextRuleSetValidator;
    }

    private bool IsDisposed { get; set; }

    protected Guid PuzzleId { get; }

    [ObservableProperty]
    private string? _text;

    [ObservableProperty]
    private IRuleSetValidator<AnswerInputRuleSet, string?> _textRuleSetValidator;

    [RelayCommand]
    protected virtual async Task TextChangedAsync()
    {
        await RefreshDesignerAsync();

        await _mediator.Publish(new RefreshDesignerNotification());
    }

    public virtual async Task CreateAsync(string initalText)
    {
        Text = initalText;

        await TextRuleSetValidator.ValidateAsync(Text);

        await RefreshDesignerAsync();
    }

    public async Task Handle(WidthChangedNotification notification, CancellationToken cancellationToken)
    {
        if (!IsDisposed)
        {
            await RefreshDesignerAsync();
        }
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
