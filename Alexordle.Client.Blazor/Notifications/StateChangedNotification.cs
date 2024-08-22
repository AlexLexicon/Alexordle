using Alexordle.Client.Blazor.Models;
using MediatR;

namespace Alexordle.Client.Blazor.Notifications;
public record class StateChangedNotification(SpecialMessages? SpecialMessage = null) : INotification;
