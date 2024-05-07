// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BlazorHybrid.Core;
using BlazorHybrid.Maui.Shared;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 服务扩展类
/// </summary>
public static class SharedServiceCollectionExtensions
{


    /// <summary>
    /// 服务扩展类,<para></para>
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

}

