using Alexordle.Client.Application.Database;
using Alexordle.Client.Application.Extensions;
using Alexordle.Client.Application.Services;
using Alexordle.Client.Blazor.Extensions;
using Alexordle.Client.Blazor.Presentation;
using Alexordle.Client.Blazor.Presentation.Services;
using Lexicom.Concentrate.Blazor.WebAssembly.Amenities.Extensions;
using Lexicom.Concentrate.Blazor.WebAssembly.Amenities.Services;
using Lexicom.Concentrate.Supports.Blazor.WebAssembly.Extensions;
using Lexicom.DependencyInjection.Primitives.Extensions;
using Lexicom.DependencyInjection.Primitives.For.Blazor.WebAssembly.Extensions;
using Lexicom.Mvvm.Amenities.Extensions;
using Lexicom.Mvvm.For.Blazor.WebAssembly.Extensions;
using Lexicom.Supports.Blazor.WebAssembly.Extensions;
using Lexicom.Validation.Amenities.Extensions;
using Lexicom.Validation.For.Blazor.WebAssembly.Extensions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Configuration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

builder.Lexicom(l =>
{
    l.AddMvvm(mvvm =>
    {
        mvvm.AddMediatR(mr =>
        {
            mr.AddViewModels();
        });

        mvvm.AddViewModels();
    });

    l.AddValidation(v =>
    {
        v.AddAmenities();
        v.AddViewModels();
    });

    l.AddPrimitives(p =>
    {
        p.AddGuidProvider();
        p.AddTimeProvider();
    });

    l.Concentrate(c =>
    {
        c.AddAmenities();
    });
});

builder.Services.AddApplication();
builder.Services.AddViewModels();

builder.Services.AddSingleton<IWordListsProvider, WordListProvider>();
builder.Services.AddSingleton<IUrlService, UrlService>();
builder.Services.AddHttpClient("wwwroot", (sp, hc) =>
{
    var navigationService = sp.GetRequiredService<INavigationService>();
    string baseUrl = navigationService.GetBaseUrlAsync().Result;

    hc.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddDbContextFactory<AlexordleDbContext>(options =>
{
    string connectionString = $"DataSource=file:mbalexorle?mode=memory&cache=shared";

    options.UseSqlite(connectionString);
});

var app = builder.Build();

app.UsePeriodicNotificator(TimeSpan.FromSeconds(0.10));

await app.RunAsync();
