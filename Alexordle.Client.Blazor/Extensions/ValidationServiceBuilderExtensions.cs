using Lexicom.Validation;
using Lexicom.Validation.Extensions;

namespace Alexordle.Client.Blazor.Extensions;
public static class ValidationServiceBuilderExtensions
{
    public static void AddViewModels(this IValidationServiceBuilder builder)
    {
        builder.AddRuleSets<AssemblyScanMarker>();
        builder.AddValidators<AssemblyScanMarker>();
        builder.AddTransformers<AssemblyScanMarker>();
    }
}
