﻿using Alexordle.Client.Blazor.Factories;
using Alexordle.Client.Blazor.Validations;
using Microsoft.Extensions.DependencyInjection;

namespace Alexordle.Client.Blazor.Extensions;
public static class ServiceCollectionExtensions
{
    public static void AddViewModels(this IServiceCollection services)
    {
        services.AddSingleton<IInputViewModelFactory, InputViewModelFactory>();
        services.AddSingleton<DesignerAnswersCountValidation>();
        services.AddSingleton<DesignerWidthValidation>();
    }
}
