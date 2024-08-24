using Alexordle.Client.Application.Models;
using Alexordle.Client.Blazor.Models;
using MediatR;

namespace Alexordle.Client.Blazor.Notifications;
public record class GeneratePalleteNotification(long InitiatedUtcTick, Guid PuzzleId, State? State = null, SpecialMessages? SpecialMessage = null, bool IsDesigner = false) : INotification;
