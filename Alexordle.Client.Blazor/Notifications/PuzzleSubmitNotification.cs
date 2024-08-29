using MediatR;

namespace Alexordle.Client.Blazor.Notifications;
public record class PuzzleSubmitNotification(Guid PuzzleId) : INotification;
