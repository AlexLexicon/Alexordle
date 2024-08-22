using Microsoft.Extensions.DependencyInjection;

namespace Alexordle.Client.Blazor.Extensions;
public static class MediatRServiceConfigurationExtensions
{
    public static void AddViewModels(this MediatRServiceConfiguration builder)
    {
        builder.RegisterServicesFromAssemblyContaining<AssemblyScanMarker>();
    }
}
