// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BlazorHybrid.Components;
using BootstrapBlazor.Components;
using BootstrapBlazor.WebAPI.Services;
using Densen.DataAcces.FreeSql;
using Densen.DataAcces.MemoryData;
using System.Globalization;
#if WINDOWS
using Windows.Storage;
#endif

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 服务扩展类
/// </summary>
public static class SharedServiceCollectionExtensions
{

    /// <summary>
    /// Maui服务扩展类,<para></para>
    /// 包含各平台差异实现
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddMauiFeatureService(this IServiceCollection services)
    {

        services.AddSharedExtensions();
        services.AddSingleton<BluetoothLEServices>();
        services.AddSingleton<INativeFeatures, MauiFeatureService>();
        return services;
    }

    /// <summary>
    /// 服务扩展类,<para></para>
    /// 包含各平台差异实现
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSharedExtensions(this IServiceCollection services)
    {
        string UploadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "uploads");
        if (!Directory.Exists(UploadPath)) Directory.CreateDirectory(UploadPath);

        var cultureInfo = new CultureInfo("zh-CN");
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

        services.AddScoped<States>();
        services.AddScoped<DataService>();

        //添加FreeSql服务

#if WINDOWS
        string dbpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "hybrid.db");
#elif ANDROID || IOS || MACCATALYST
        string dbpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "hybrid.db");
#else
        string dbpath = "hybrid.db";
#endif

        services.AddFreeSql(option =>
        {
            option
                 //#if DEBUG
                 .UseConnectionString(FreeSql.DataType.Sqlite, $"Data Source={dbpath};")
                 .UseAutoSyncStructure(true)
                 //调试sql语句输出
                 .UseMonitorCommand(cmd => System.Console.WriteLine(cmd.CommandText + Environment.NewLine))
                 .UseNoneCommandParameter(true);

        }, configEntityPropertyImage: true);

        services.AddDensenExtensions();
        services.ConfigureJsonLocalizationOptions(op =>
        {
            // 忽略文化信息丢失日志
            op.IgnoreLocalizerMissing = true;

        });
        //附加查询条件数据库操作服务
        services.AddTransient(typeof(FreeSqlDataService<>));
        services.AddTransient<INativeFeatures, NullFeatureService>();
        services.AddScoped<ILookupService, AppLookupService>();
        services.AddScoped<ICookie, CookieService>();
        services.AddScoped<IStorage, StorageService>();

        return services;
    }

}

