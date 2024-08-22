using Alexordle.Client.Blazor.ViewModels.Designer;
using Alexordle.Client.Blazor.ViewModels.Grid;
using Alexordle.Client.Blazor.ViewModels.Keyboard;
using CommunityToolkit.Mvvm.ComponentModel;
using Lexicom.DependencyInjection.Amenities.Extensions;
using Lexicom.Mvvm;
using Lexicom.Mvvm.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Alexordle.Client.Blazor.Extensions;
public static class MvvmServiceBuilderExtensions
{
    public static void AddViewModels(this IMvvmServiceBuilder builder)
    {
        builder.Services
            .AssemblyScan<AssemblyScanMarker>()
            .For<ObservableObject>()
            //transient
            .TryToRegister<AnswerInputViewModel>(vm =>
            {
                builder.AddViewModel(vm, ServiceLifetime.Transient);
            })
            .ThenTryToRegister<ClueInputViewModel>(vm =>
            {
                builder.AddViewModel(vm, ServiceLifetime.Transient);
            })
            .ThenTryToRegister<CellViewModel>(vm =>
            {
                builder.AddViewModel(vm, ServiceLifetime.Transient);
            })
            .ThenTryToRegister<RowViewModel>(vm =>
            {
                builder.AddViewModel(vm, ServiceLifetime.Transient);
            })
            .ThenTryToRegister<KeyViewModel>(vm =>
            {
                builder.AddViewModel(vm, ServiceLifetime.Transient);
            })
            //singleton
            .OtherwiseRegister(vm =>
            {
                builder.AddViewModel(vm);
            });
    }
}
