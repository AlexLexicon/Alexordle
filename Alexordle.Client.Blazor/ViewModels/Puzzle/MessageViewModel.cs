using Alexordle.Client.Application.Database.Entities;
using Alexordle.Client.Application.Services;
using Alexordle.Client.Blazor.Models;
using Alexordle.Client.Blazor.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using Lexicom.Extensions.Strings;
using MediatR;

namespace Alexordle.Client.Blazor.ViewModels.Game;
public partial class MessageViewModel : ObservableObject, INotificationHandler<SetMessageNotification>
{
    private readonly IPuzzleService _puzzleService;

    public MessageViewModel(IPuzzleService puzzleService)
    {
        _puzzleService = puzzleService;
    }

    [ObservableProperty]
    private string? _text;

    public async Task Handle(SetMessageNotification notification, CancellationToken cancellationToken)
    {
        Text = await GetTextAsync(notification.PuzzleId, notification.Message);
    }

    private async Task<string> GetTextAsync(Guid puzzleId, Message message)
    {
        if (message is Message.CopiedShare)
        {
            return "Result copied to clipboard!";
        }

        if (message is Message.IncompeleteGuess)
        {
            return "Type more letters.";
        }

        if (message is Message.IncorrectSpelling)
        {
            return  "That is not in the word list.";
        }

        if (message is Message.AlreadyGuessed)
        {
            return  "You have already guessed that.";
        }

        Puzzle puzzle = await _puzzleService.GetPuzzleAsync(puzzleId);

        int remainingAnswers = puzzle.TotalAnswers - puzzle.CurrentAnswers;

        if (puzzle.IsFinished)
        {
            if (remainingAnswers is <= 0)
            {
                return "You won!";
            }
            else
            {
                return "You lost.";
            }
        }

        return await GetWordsRemainMessageAsync(remainingAnswers);
    }

    private Task<string> GetWordsRemainMessageAsync(int remainingAnswers)
    {
        bool isSinglular = remainingAnswers is 1;

        string remaining = remainingAnswers.GetCountWord();

        string text = $"{remaining.CapitalizeFirstLetter()} word{(isSinglular ? "" : "s")} remain{(isSinglular ? "s" : "")}.";

        return Task.FromResult(text);
    }
}
