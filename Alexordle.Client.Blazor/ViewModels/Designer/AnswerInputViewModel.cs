using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Services;
using Alexordle.Client.Blazor.Notifications;
using Alexordle.Client.Blazor.Validations.RuleSets;
using Lexicom.Validation;
using MediatR;

namespace Alexordle.Client.Blazor.ViewModels.Designer;
public partial class AnswerInputViewModel : AbstractInputViewModel
{
    private readonly IAnswerService _answerService;
    private readonly IGuessService _guessService;
    private readonly ILetterService _letterService;
    private readonly IPuzzleService _puzzleService;

    public AnswerInputViewModel(
        Guid puzzleId,
        IMediator mediator,
        IRuleSetValidator<AnswerInputRuleSet, string?> answerTextRuleSetValidator,
        IAnswerService answerService,
        IGuessService guessService,
        ILetterService letterService,
        IPuzzleService puzzleService) : base(puzzleId, mediator, answerTextRuleSetValidator)
    {
        _answerService = answerService;
        _guessService = guessService;
        _letterService = letterService;
        _puzzleService = puzzleService;
    }

    private Guid? AnswerWordId { get; set; }
    private Guid GuessWordId { get; set; }

    public override async Task CreateAsync(string initalText)
    {
        await CreateGuessAsync();

        await base.CreateAsync(initalText);
    }

    protected override async Task RemoveAsync()
    {
        await _guessService.DeleteGuessAsync(GuessWordId);

        await base.RemoveAsync();
    }

    protected override async Task DeleteAsync()
    {
        if (AnswerWordId is not null)
        {
            await _answerService.DeleteAnswerAsync(AnswerWordId.Value);

            AnswerWordId = null;
        }
    }

    protected override INotification GetRemoveInputNotification()
    {
        return new RemoveAnswerInputNotification(this);
    }

    protected override async Task RefreshDesignerAsync()
    {
        await base.RefreshDesignerAsync();

        if (Text is not null)
        {
            Answer answer = await _answerService.CreateAnswerAsync(PuzzleId, Text);

            AnswerWordId = answer.WordId;

            await WriteGuessAsync();

            await TryCommitAsync();
        }
    }

    private async Task TryCommitAsync()
    {
        Puzzle puzzle = await _puzzleService.GetPuzzleAsync(PuzzleId);

        int count = await _letterService.CountLettersAsync(GuessWordId);

        if (count == puzzle.Width)
        {
            await _guessService.SubmitGuessAsync(GuessWordId);
        }
        else
        {
            await _guessService.DeleteGuessAsync(GuessWordId);

            await CreateGuessAsync();

            await WriteGuessAsync();
        }
    }

    private async Task CreateGuessAsync()
    {
        Guess guess = await _guessService.CreateGuessAsync(PuzzleId);

        GuessWordId = guess.WordId;
    }

    private async Task WriteGuessAsync()
    {
        if (Text is not null)
        {
            await _letterService.DeleteLettersAsync(GuessWordId);

            foreach (char character in Text)
            {
                await _letterService.AppendLetterAsync(GuessWordId, character);
            }
        }
    }
}
