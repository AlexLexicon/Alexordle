using Alexordle.Client.Blazor.ViewModels.Designer;
using MediatR;

namespace Alexordle.Client.Blazor.Notifications;
public record class RemoveAnswerInputNotification(AnswerInputViewModel AnswerInputViewModel) : INotification;
