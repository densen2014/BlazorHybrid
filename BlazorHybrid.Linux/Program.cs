// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BlazorHybrid.Shared;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using SpiderEye;
using Application = SpiderEye.Application;
using OperatingSystem = SpiderEye.OperatingSystem;
using Window = SpiderEye.Window;

#if WINDOWS
using SpiderEye.Windows;
#else
using SpiderEye.Linux;
using SpiderEye.Mac;
#endif

internal class Program
{

    #region SSR host
    private static void MainSsr(string[] args)
    {

        AppState _appState = new();
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddResponseCompression(options =>
        {
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
            options.MimeTypes =
            ResponseCompressionDefaults.MimeTypes.Concat(
                            new[] { "image/svg+xml" });
        });
        builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Optimal;
        });

        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor(a =>
        {
            a.DetailedErrors = true;
            a.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(2);
            a.MaxBufferedUnacknowledgedRenderBatches = 20;
            a.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(10);
        }).AddHubOptions(o =>
        {
            o.EnableDetailedErrors = true;
            //单个传入集线器消息的最大大小。默认 32 KB	
            o.MaximumReceiveMessageSize = null;
            //可为客户端上载流缓冲的最大项数。 如果达到此限制，则会阻止处理调用，直到服务器处理流项。
            o.StreamBufferCapacity = 20;
        });
        builder.Services.AddSharedExtensions();
        builder.Services.AddSingleton(_appState);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            //app.UseHsts();
        }
        app.UseResponseCompression();

        //app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseDirectoryBrowser(new DirectoryBrowserOptions()
        {
            RequestPath = new PathString("/pic")
        });
        app.UseRouting();

        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        app.RunAsync();
    }
    #endregion

    [STAThread]
    public static void Main(string[] args)
    {
        MainSsr(args);

        #region CrossUI

#if WINDOWS
        WindowsApplication.Init();
#else
        if (Application.OS == OperatingSystem.Linux)
        {
            LinuxApplication.Init();
        }
        else if (Application.OS == OperatingSystem.MacOS)
        {
            MacApplication.Init();
        }
#endif
        var icon = AppIcon.FromFile("icon", AppDomain.CurrentDomain.BaseDirectory);

#if WINDOWS
        using var statusIcon = new StatusIcon();
#endif
        try
        {
            using var window = new Window();
            window.Title = "BlazorHybrid.Linux";
            window.UseBrowserTitle = true;
            window.EnableScriptInterface = true;
            window.CanResize = true;
            window.BackgroundColor = "#303030";
            window.Icon = icon;

#if DEBUG
            window.EnableDevTools = true;
#endif

            //var bridge = new CrossBridgeService();
            //Application.AddGlobalHandler(bridge);
            Application.Run(window, "http://localhost:5000");
        }
        catch (Exception e)
        {
            if (e.HResult == -2146233079 && Application.OS == OperatingSystem.Windows)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\n\n\n Windows 环境请使用 net9.0-windows10.0.17763 环境执行.\n\nIn windows please use target framework with 'net9.0-windows10.0.17763'. \n\n\n\n");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                throw;
            }

        }
        #endregion

    }

}

