using Alexordle.Client.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Alexordle.Client.Application.Extensions;
public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IAnswerService, AnswerService>();
        services.AddSingleton<IClueService, ClueService>();
        services.AddSingleton<IDictionaryService, DictionaryService>();
        services.AddSingleton<IGuessService, GuessService>();
        services.AddSingleton<IHintService, HintService>();
        services.AddSingleton<IHunchService, HunchService>();
        services.AddSingleton<IPersistenceService, PersistenceService>();
        services.AddSingleton<IPuzzleService, PuzzleService>();
        services.AddSingleton<IShareService, ShareService>();
        services.AddSingleton<ISerializationService, SerializationService>();
    }
}
