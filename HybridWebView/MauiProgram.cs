// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace HybridWebView
{
    public static class MauiProgram
    {
        [NotNull]
        public static IServiceProvider? Services { get; private set; }
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddMauiFeatureService();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
            builder.Services.AddLogging(logging =>
            {
#if WINDOWS && NET7_0_OR_GREATER
				logging.AddDebug();
#elif NET7_0_OR_GREATER
                logging.AddConsole();
#endif

                // Enable maximum logging for BlazorWebView
                logging.AddFilter("Microsoft.AspNetCore.Components.WebView", LogLevel.Trace);
            });
#endif
            var app = builder.Build();
            Services = app.Services;

            return app;
        }
    }
}
