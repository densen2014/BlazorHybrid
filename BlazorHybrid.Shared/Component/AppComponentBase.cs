// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BlazorHybrid.Core;
using BootstrapBlazor.Components;
using BootstrapBlazor.WebAPI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;
using Color = BootstrapBlazor.Components.Color;

namespace BlazorHybrid.Shared;


/// <summary>
/// 组件基类
/// </summary>
public abstract partial class AppComponentBase : ComponentBase, IDisposable
{
    [Inject, NotNull] protected DataService? DataService { get; set; }
    [Inject, NotNull] protected States? States { get; set; }

    [Inject, NotNull] protected ToastService? ToastService { get; set; }
    [Inject, NotNull] protected MessageService? MessageService { get; set; }
    [Inject, NotNull] protected IJSRuntime? JS { get; set; }
    [Inject, NotNull] protected ICookie? Cookie { get; set; }
    [Inject, NotNull] protected IStorage? Storage { get; set; }
    [Inject, NotNull] protected INativeFeatures? Tools { get; set; }
    [Inject, NotNull] protected NavigationManager? NavigationManager { get; set; }
    protected bool IsBusy { get; set; }

    [Parameter]
    public EventCallback Changed { get; set; }

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
