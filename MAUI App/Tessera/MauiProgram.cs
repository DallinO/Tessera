using Microsoft.Extensions.Logging;

namespace Tessera
{
    /***************************************************************************
     * This is the main entry point of the application.
    ***************************************************************************/
    public static class MauiProgram
    {
        /***********************************************************************
         * CREATE MAUI APP
         * This method initialized the MAUI application by setting up services,
         * font, and other configurations
         **********************************************************************/
        public static MauiApp CreateMauiApp()
        {
            // Create a variable called `builder` and initialize it using the
            // MauiApp.CreateBuilder(). This static  method creates and returns
            // a new instance of MauiAppBuilder, which is used to configure the
            // MAUI application.
            var builder = MauiApp.CreateBuilder();

            // Execute the .UseMauiApp<App> method which is used to register
            // the `App` class defined in the App.xaml & App.axml.cs file.
            builder
                .UseMauiApp<App>()
                // Execute the ConfigureFonts method which allows you to
                // configure custom fonts for the application.
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // Adds the Blazor WebView service to the MAUI application,
            // enabling the use of Blazor components within the MAUI app.
            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            // Adds developer tools for Blazor WebView, which can help with
            // debugging and development.
            builder.Services.AddBlazorWebViewDeveloperTools();
            // Adds a debug logger to the logging system, which outputs debug
            // information to the console or debug window.
            builder.Logging.AddDebug();
#endif
            // Register the HttpClient and the ApiService
            builder.Services.AddHttpClient<IApiService, ApiService>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:5206");
                // Configure other settings as needed
            });
            builder.Services.AddScoped<ILibraryService, LibraryService>();

            // Finalizes the configuration, builds, then returns the MauiApp
            // instance.
            return builder.Build();
        }
    }
}
