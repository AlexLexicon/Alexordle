using Alexordle.Client.Blazor.Models;
using MediatR;

namespace Alexordle.Client.Blazor.Notifications;
public record class SetMessageNotification(Guid PuzzleId, Message Message) : INotification
{
}
