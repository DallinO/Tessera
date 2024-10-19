using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Tessera.Web.Handlers;
using Blazored.SessionStorage;
using Blazor_Web_App;
using Tessera.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddTransient<AuthenticationHandler>();

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddHttpClient("ServerApi")
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://localhost:7031"))
                .AddHttpMessageHandler<AuthenticationHandler>();


builder.Services.AddSingleton<ILibraryService, LibraryService>();
builder.Services.AddSingleton<IApiService, ApiService>();
builder.Services.AddSingleton<IViewService, ViewService>();

builder.Services.AddBlazoredSessionStorageAsSingleton();

await builder.Build().RunAsync();

//builder.Services.AddHttpClient<IApiService, ApiService>(client =>
//{
//    client.BaseAddress = new Uri("https://localhost:7031");
//    // Configure other settings as needed
//})
//    .AddHttpMessageHandler<AuthenticationHandler>();