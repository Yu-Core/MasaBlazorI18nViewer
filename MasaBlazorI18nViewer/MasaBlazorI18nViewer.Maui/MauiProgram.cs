using MasaBlazorI18nViewer.Maui.Extensions;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

namespace MasaBlazorI18nViewer.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif
            builder.Services.AddMasaBlazor();

            builder.Services.AddDependencyInjection();

            return builder.Build();
        }
    }
}