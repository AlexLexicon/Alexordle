using MediatR;

namespace Alexordle.Client.Blazor.Notifications;
public record class PuzzleUpdateNotification(Guid PuzzleId) : INotification;
