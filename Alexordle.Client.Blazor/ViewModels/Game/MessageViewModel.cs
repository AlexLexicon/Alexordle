using Alexordle.Client.Blazor.Models;
using Alexordle.Client.Blazor.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using Lexicom.Extensions.Strings;
using MediatR;

namespace Alexordle.Client.Blazor.ViewModels.Game;
public partial class MessageViewModel : ObservableObject, INotificationHandler<MessageNotification>
{
    [ObservableProperty]
    private string? _text;

    public async Task Handle(MessageNotification notification, CancellationToken cancellationToken)
    {
        if (notification.SpecialMessage is SpecialMessages.ShareCopied)
        {
            Text = "Result copied to clipboard!";

            return;
        }

        if (notification.SpecialMessage is SpecialMessages.IncompeleteWord)
        {
            Text = "Type more letters.";

            return;
        }

        if (notification.SpecialMessage is SpecialMessages.NotSpelledCorrectly)
        {
            Text = "That is not in the word list.";

            return;
        }

        if (notification.SpecialMessage is SpecialMessages.AlreadyGuessed)
        {
            Text = "You have already guessed that.";

            return;
        }

        if (notification.State.IsVictory)
        {
            Text = "You won!";

            return;
        }

        if (notification.State.IsDefeat)
        {
            Text = "You lost.";

            return;
        }

        Text = await GetWordsRemainMessageAsync(notification.State.RemainingAnswers);
    }

    private Task<string> GetWordsRemainMessageAsync(int remainingAnswers)
    {
        bool isSinglular = remainingAnswers is 1;
        string remaining = remainingAnswers.GetCountWord();

        string text = $"{remaining.CapitalizeFirstLetter()} word{(isSinglular ? "" : "s")} remain{(isSinglular ? "s" : "")}.";

        return Task.FromResult(text);
    }
}
