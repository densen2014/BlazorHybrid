﻿// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BlazorHybrid.Maui.Shared;
using BootstrapBlazor.Components;
using BootstrapBlazor.WebAPI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;
using Color = BootstrapBlazor.Components.Color;

namespace test1.Component.Shared;


/// <summary>
/// 组件基类
/// </summary>
public abstract partial class AppComponentBase : ComponentBase, IDisposable
{

    [Inject, NotNull] protected ToastService? ToastService { get; set; }
    [Inject, NotNull] protected MessageService? MessageService { get; set; }
    [Inject, NotNull] protected IJSRuntime? JS { get; set; }
    [Inject, NotNull] protected IStorage? Storage { get; set; }
    [Inject, NotNull] protected BluetoothLEServices? Tools { get; set; }
    [Inject, NotNull] protected NavigationManager? NavigationManager { get; set; }
    protected bool IsBusy { get; set; }

    [Parameter]
    public EventCallback Changed { get; set; }

    ///// <summary>
    ///// 获得/设置 客户端屏幕宽度
    ///// </summary>
    //public BreakPoint ScreenSize { get; set; }

    ///// <summary>
    ///// 获得 渲染模式
    ///// </summary>
    //public bool ShowText => ScreenSize > BreakPoint.Medium;
    //public virtual Task OnBreakPointChanged(BreakPoint size)
    //{
    //    ScreenSize = size;
    //    StateHasChanged();
    //    return Task.CompletedTask;
    //}

    /// <summary>
    /// Dispose 方法
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {

    }

    /// <summary>
    /// Dispose 方法
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
    }

    protected virtual Task BackToHome()
    {
        NavigationManager.NavigateTo("/");
        return Task.CompletedTask;
    }

    [NotNull]
    protected Message? Message { get; set; }

    protected async Task ShowBottomMessage(string message, bool error = false)
    {
        await MessageService.Show(new MessageOption()
        {
            Content = message,
            Icon = "fa-solid fa-circle-info",
            Color = error ? Color.Warning : Color.Primary
        }, Message);
    }


}
