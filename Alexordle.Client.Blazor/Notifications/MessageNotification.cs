using Alexordle.Client.Application.Models;
using Alexordle.Client.Blazor.Models;
using MediatR;

namespace Alexordle.Client.Blazor.Notifications;
public record class MessageNotification(Guid PuzzleId, State State, SpecialMessages? SpecialMessage) : INotification;
