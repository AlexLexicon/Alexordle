using Alexordle.Client.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Alexordle.Client.Application.Extensions;
public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IAnswerService, AnswerService>();
        services.AddSingleton<IClueService, ClueService>();
        services.AddSingleton<ISerializationService, CustomSerializationService>();
        services.AddSingleton<IDictionaryService, DictionaryService>();
        services.AddSingleton<IGameService, GameService>();
        services.AddSingleton<IGuessService, GuessService>();
        services.AddTransient<IHighlightService, HighlightService>();
        services.AddSingleton<IHighlightServiceFactory, HighlightServiceFactory>();
        services.AddSingleton<ILetterService, LetterService>();
        services.AddSingleton<IPalleteService, PalleteService>();
        services.AddSingleton<IPuzzleService, PuzzleService>();
        services.AddSingleton<IShareService, ShareService>();
        services.AddSingleton<IStateService, StateService>();
        services.AddSingleton<ITimestampService, TimestampService>();
        services.AddSingleton<IWordService, WordService>();
    }
}
