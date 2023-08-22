// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using bh003_ble.Data;
using Microsoft.Extensions.Logging;
using BlazorHybrid.Maui.Shared;
using BootstrapBlazor.WebAPI.Services;

namespace bh003_ble
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<WeatherForecastService>();
            builder.Services.AddDensenExtensions();
            builder.Services.ConfigureJsonLocalizationOptions(op =>
            {
                // 忽略文化信息丢失日志
                op.IgnoreLocalizerMissing = true;

            });
            builder.Services.AddSingleton<BluetoothLEServices>();
            builder.Services.AddScoped<IStorage, StorageService>();

            return builder.Build();
        }
    }
}
