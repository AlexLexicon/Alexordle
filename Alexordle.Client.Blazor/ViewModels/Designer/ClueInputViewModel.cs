using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Services;
using Alexordle.Client.Blazor.Notifications;
using Alexordle.Client.Blazor.Validations.RuleSets;
using Lexicom.DependencyInjection.Primitives;
using Lexicom.Validation;
using MediatR;

namespace Alexordle.Client.Blazor.ViewModels.Designer;
public partial class ClueInputViewModel : AbstractInputViewModel
{
    private readonly IClueService _clueService;

    public ClueInputViewModel(
        Guid puzzleId,
        IMediator mediator,
        ITimeProvider timeProvider,
        IRuleSetValidator<AnswerInputRuleSet, string?> answerTextRuleSetValidator,
        IClueService clueService) : base(puzzleId, mediator, timeProvider, answerTextRuleSetValidator)
    {
        _clueService = clueService;
    }

    private Guid? ClueWordId { get; set; }

    public override Task CreateAsync(string initalText)
    {
        return base.CreateAsync(initalText);
    }

    protected override async Task DeleteAsync()
    {
        if (ClueWordId is not null)
        {
            await _clueService.DeleteClueAsync(ClueWordId.Value);

            ClueWordId = null;
        }
    }

    protected override INotification GetRemoveInputNotification()
    {
        return new RemoveClueInputNotification(this);
    }

    protected override async Task RefreshDesignerAsync()
    {
        await base.RefreshDesignerAsync();

        if (Text is not null)
        {
            Clue clue = await _clueService.CreateClueAsync(PuzzleId, Text);

            ClueWordId = clue.WordId;
        }
    }
}
