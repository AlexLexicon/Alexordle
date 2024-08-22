using Alexordle.Client.Application.Models;
using MediatR;

namespace Alexordle.Client.Blazor.Notifications;
public record class CharacterHighlightNotification(char Character, Highlights Highlight, bool IsFinished) : INotification;
