using Alexordle.Client.Blazor.ViewModels.Designer;
using MediatR;

namespace Alexordle.Client.Blazor.Notifications;
public record class RemoveListInputViewModelNotification(ListInputViewModel ListInputViewModel) : INotification;
