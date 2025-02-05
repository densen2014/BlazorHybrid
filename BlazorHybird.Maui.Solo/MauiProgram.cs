// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BlazorHybrid.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using System.Diagnostics;

namespace BlazorHybird.Maui.Solo;

public static class MauiProgram
{

    public static IServiceProvider? Services { get; private set; }
    public static readonly AppState _appState = new();

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            })
            .ConfigureEssentials(essentials =>
            {
                essentials
                    .AddAppAction("app_info", "App信息", icon: "on")
                    .AddAppAction("battery_info", "电池")
                    .AddAppAction("nfc", "NFC")
                    .OnAppAction(App.HandleAppActions);
            })

        #region 生命周期

          .ConfigureLifecycleEvents(events =>
          {
              events.AddEvent<Action<string>>("CustomEventName", value => LogEvent("CustomEventName"));

#if __ANDROID__
                 // Log everything in this one
                 events.AddAndroid(android => android
                     .OnActivityResult((a, b, c, d) => LogEvent(nameof(AndroidLifecycle.OnActivityResult), b.ToString()))
                     .OnBackPressed((a) => LogEvent(nameof(AndroidLifecycle.OnBackPressed)) && false)
                     .OnConfigurationChanged((a, b) => LogEvent(nameof(AndroidLifecycle.OnConfigurationChanged)))
                     .OnCreate((a, b) => LogEvent(nameof(AndroidLifecycle.OnCreate)))
                     .OnDestroy((a) => LogEvent(nameof(AndroidLifecycle.OnDestroy)))
                     .OnNewIntent((a, b) => LogEvent(nameof(AndroidLifecycle.OnNewIntent)))
                     .OnPause((a) => LogEvent(nameof(AndroidLifecycle.OnPause)))
                     .OnPostCreate((a, b) => LogEvent(nameof(AndroidLifecycle.OnPostCreate)))
                     .OnPostResume((a) => LogEvent(nameof(AndroidLifecycle.OnPostResume)))
                     .OnRequestPermissionsResult((a, b, c, d) => LogEvent(nameof(AndroidLifecycle.OnRequestPermissionsResult)))
                     .OnRestart((a) => LogEvent(nameof(AndroidLifecycle.OnRestart)))
                     .OnRestoreInstanceState((a, b) => LogEvent(nameof(AndroidLifecycle.OnRestoreInstanceState)))
                     .OnResume((a) => LogEvent(nameof(AndroidLifecycle.OnResume)))
                     .OnSaveInstanceState((a, b) => LogEvent(nameof(AndroidLifecycle.OnSaveInstanceState)))
                     .OnStart((a) => LogEvent(nameof(AndroidLifecycle.OnStart)))
                     .OnStop((a) => LogEvent(nameof(AndroidLifecycle.OnStop))));

                 // Add some cool features/things
                 var shouldPreventBack = 1;
                 events.AddAndroid(android => android
                     .OnResume(a =>
                     {
                         LogEvent(nameof(AndroidLifecycle.OnResume), "shortcut");
                     })
                     .OnBackPressed(a => LogEvent(nameof(AndroidLifecycle.OnBackPressed), "shortcut") && (shouldPreventBack-- > 0))
                     .OnRestoreInstanceState((a, b) =>
                     {
                         LogEvent(nameof(AndroidLifecycle.OnRestoreInstanceState), "shortcut");

                         Debug.WriteLine($"{b.GetString("test2", "fail")} == {b.GetBoolean("test", false)}");
                     })
                     .OnSaveInstanceState((a, b) =>
                     {
                         LogEvent(nameof(AndroidLifecycle.OnSaveInstanceState), "shortcut");

                         b.PutBoolean("test", true);
                         b.PutString("test2", "yay");
                     }));
#elif __IOS__
                 // Log everything in this one
                 events.AddiOS(ios => ios
                     .ContinueUserActivity((a, b, c) => LogEvent(nameof(iOSLifecycle.ContinueUserActivity)) && false)
                     .DidEnterBackground((a) => LogEvent(nameof(iOSLifecycle.DidEnterBackground)))
                     .FinishedLaunching((a, b) => LogEvent(nameof(iOSLifecycle.FinishedLaunching)) && true)
                     .OnActivated((a) => LogEvent(nameof(iOSLifecycle.OnActivated)))
                     .OnResignActivation((a) => LogEvent(nameof(iOSLifecycle.OnResignActivation)))
                     .OpenUrl((a, b, c) => LogEvent(nameof(iOSLifecycle.OpenUrl)) && false)
                     .PerformActionForShortcutItem((a, b, c) => LogEvent(nameof(iOSLifecycle.PerformActionForShortcutItem)))
                     .WillEnterForeground((a) => LogEvent(nameof(iOSLifecycle.WillEnterForeground)))
                     .ApplicationSignificantTimeChange((a) => LogEvent(nameof(iOSLifecycle.ApplicationSignificantTimeChange)))
                     .WillTerminate((a) => LogEvent(nameof(iOSLifecycle.WillTerminate))));
#elif WINDOWS
              // Log everything in this one
              events.AddWindows(windows => windows
                  // .OnPlatformMessage((a, b) => 
                  //	LogEvent(nameof(WindowsLifecycle.OnPlatformMessage)))
                  .OnActivated((a, b) => LogEvent(nameof(WindowsLifecycle.OnActivated)))
                  .OnClosed((a, b) => LogEvent(nameof(WindowsLifecycle.OnClosed)))
                  .OnLaunched((a, b) => LogEvent(nameof(WindowsLifecycle.OnLaunched)))
                  .OnVisibilityChanged((a, b) => LogEvent(nameof(WindowsLifecycle.OnVisibilityChanged)))
                  .OnWindowCreated((del) => { del.ExtendsContentIntoTitleBar = true; }));
#elif TIZEN
					events.AddTizen(tizen => tizen
						.OnAppControlReceived((a, b) => LogEvent(nameof(TizenLifecycle.OnAppControlReceived)))
						.OnCreate((a) => LogEvent(nameof(TizenLifecycle.OnCreate)))
						.OnDeviceOrientationChanged((a, b) => LogEvent(nameof(TizenLifecycle.OnDeviceOrientationChanged)))
						.OnLocaleChanged((a, b) => LogEvent(nameof(TizenLifecycle.OnLocaleChanged)))
						.OnLowBattery((a, b) => LogEvent(nameof(TizenLifecycle.OnLowBattery)))
						.OnLowMemory((a, b) => LogEvent(nameof(TizenLifecycle.OnLowMemory)))
						.OnPause((a) => LogEvent(nameof(TizenLifecycle.OnPause)))
						.OnPreCreate((a) => LogEvent(nameof(TizenLifecycle.OnPreCreate)))
						.OnRegionFormatChanged((a, b) => LogEvent(nameof(TizenLifecycle.OnRegionFormatChanged)))
						.OnResume((a) => LogEvent(nameof(TizenLifecycle.OnResume)))
						.OnTerminate((a) => LogEvent(nameof(TizenLifecycle.OnTerminate))));
#endif

              static bool LogEvent(string eventName, string type = null)
              {
                  Debug.WriteLine($"Lifecycle event: {eventName}{(type == null ? "" : $" ({type})")}");
                  return true;
              }
          })
        #endregion
          ;

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddSingleton(_appState);
        builder.Services.AddMauiFeatureService();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
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
