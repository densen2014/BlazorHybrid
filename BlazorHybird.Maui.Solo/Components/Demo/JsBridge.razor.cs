// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using AME;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace BlazorHybrid.Components;

public partial class JsBridge//: IAsyncDisposable
{
    string? message;
    bool BridgeEnabled;

    [Inject, NotNull]
    IJSRuntime? JS { get; set; }

    [Inject, NotNull]
    ToastService? ToastService { get; set; }

    //private IJSObjectReference? module;

    async Task GetMacAdress()
    {
        //message = await module!.InvokeAsync<string>("GetMacAdress");
        //await ToastService.Information("JS方式 macAdress", message);

        message = await JS!.InvokeAsync<string>("eval", $"localStorage.getItem('macAdress');");
        await ToastService.Information("eval macAdress", message);

        message = await JS!.InvokeAsync<string>("eval", "bridge.Func('测试')");
        await ToastService.Information("eval bridge.Func", message);
    }
    async Task OnPrint()
    {
        message = await JS!.InvokeAsync<string>("eval", $"bridge.Print('打印文本123456789')");
        await ToastService.Information("eval bridge.Print", message);

        message = await JS!.InvokeAsync<string>("eval", $"bridge.Print({ItemsPrint.ObjectToJson()})");
        await ToastService.Information("eval bridge.Print object", message);

    }
    async Task GetUserName()
    {
        message = await JS!.InvokeAsync<string>("eval", $"bridge.GetUserName()");
        await ToastService.Information("eval bridge.GetUserName", message);

    }

    string[] ItemsPrint = ["Item1", "Item2", "Item3"];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (firstRender)
            {
                BridgeEnabled = await JS!.InvokeAsync<bool>("eval", $"typeof bridge != 'undefined'");

                message = await JS!.InvokeAsync<string>("eval", $"localStorage.getItem('macAdress');");

                //这种js隔离方式不能用,暂时还没空处理
                //module = await JS!.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorHybrid.Shared/Pages/JsBridge.razor.js" + "?v=" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            }
        }
        catch (Exception e)
        {
            message = e.Message;
        }
        StateHasChanged();
    }


    //async ValueTask IAsyncDisposable.DisposeAsync()
    //{
    //    if (module is not null)
    //    {
    //        await module.DisposeAsync();
    //    }
    //}

}
