// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using bh006_NFC_tag.Data;
using BootstrapBlazor.WebAPI.Services;
using Microsoft.Extensions.Logging;

namespace bh006_NFC_tag
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
            builder.Services.AddScoped<IStorage, StorageService>();

            return builder.Build();
        }


        public static void OpenNFCXamlPage()
        {
            Application.Current.Dispatcher.Dispatch(async () =>
            {
                //await Application.Current.MainPage.Navigation.PopToRootAsync();
                //await Application.Current.MainPage.Navigation.PushAsync(Nfcs); 
                await Application.Current.MainPage.Navigation.PushModalAsync(new NfcPage());
            });
        }

    }


}
