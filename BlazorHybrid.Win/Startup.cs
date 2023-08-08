// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BlazorHybrid.Core;
using BlazorHybrid.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Web.Services.Description;

namespace BlazorHybrid.Win;

public static class Startup
{
    public static IServiceProvider? Services { get; private set; }
    public static IConfiguration? Config;
    public static readonly AppState _appState = new();

    public static void Init()
    {
        var host = Host.CreateDefaultBuilder()
                       .ConfigureServices(WireupServices)
                       .Build();
        Services = host.Services;
    }

    private static void WireupServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddLogging(c =>
        {
            c.AddDebug();
            // Enable maximum logging for BlazorWebView
            c.AddFilter("Microsoft.AspNetCore.Components.WebView", LogLevel.Trace);
        });
        services.AddWindowsFormsBlazorWebView();
        services.AddSingleton(_appState);
        services.AddSharedExtensions();
        services.AddTransient<INativeFeatures, WinFormsService>();

        //#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
//#endif
    }
}
