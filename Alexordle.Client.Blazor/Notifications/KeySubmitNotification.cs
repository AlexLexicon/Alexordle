using MediatR;

namespace Alexordle.Client.Blazor.Notifications;
public record class KeySubmitNotification(char Character) : INotification;
