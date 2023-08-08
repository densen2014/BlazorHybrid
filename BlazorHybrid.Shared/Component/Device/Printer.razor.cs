// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace BlazorHybrid.Core.Device;

/// <summary>
/// 蓝牙打印机 BT Printer 组件
/// </summary>
public partial class Printer : IAsyncDisposable
{
    [Inject] IJSRuntime? JS { get; set; }
    private IJSObjectReference? module;
    private DotNetObjectReference<Printer>? InstancePrinter { get; set; }

    /// <summary>
    /// UI界面元素的引用对象
    /// </summary>
    public ElementReference PrinterElement { get; set; }

    /// <summary>
    /// 获得/设置 打印按钮文字 默认为 打印
    /// </summary>
    [Parameter]
    [NotNull]
    public string? PrintButtonText { get; set; } = "打印";

    /// <summary>
    /// 获得/设置 PrinterOption
    /// </summary>
    [Parameter]
    public PrinterOption Opt { get; set; } = new PrinterOption();

    /// <summary>
    /// 打印指令
    /// </summary>
    /// <returns></returns>
    [DisplayName("打印指令")]
    public string? Commands { get; set; } = @"! 10 200 200 400 1
BEEP 1
PW 380
SETMAG 1 1
CENTER
TEXT 10 2 10 40 Micro Bar
TEXT 12 3 10 75 Blazor
TEXT 10 2 10 350 eMenu
B QR 30 150 M 2 U 7
MA,https://google.com
ENDQR
FORM
PRINT
";

    /// <summary>
    /// 获得/设置 状态更新回调方法
    /// </summary>
    [Parameter]
    public Func<string, Task>? OnUpdateStatus { get; set; }

    /// <summary>
    /// 获得/设置 错误更新回调方法
    /// </summary>
    [Parameter]
    public Func<string, Task>? OnUpdateError { get; set; }

    /// <summary>
    /// 可用已配对设备列表
    /// </summary>
    public List<string>? Devices;

    /// <summary>
    /// 获得/设置 显示内置UI
    /// </summary>
    [Parameter]
    public bool ShowUI { get; set; }

    /// <summary>
    /// 获得/设置 显示log
    /// </summary>
    [Parameter]
    public bool Debug { get; set; }

    /// <summary>
    /// 获得/设置 设备名称
    /// </summary>
    [Parameter]
    public string? Devicename { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (firstRender)
            {
                module = await JS!.InvokeAsync<IJSObjectReference>("import", "./_content/BootstrapBlazor.Bluetooth/lib/printer/app.js" + "?v=" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
                await module.InvokeVoidAsync("addScript", "./_content/BootstrapBlazor.Bluetooth/lib/printer/gbk.min.js");
                InstancePrinter = DotNetObjectReference.Create(this);
                //可选设置初始搜索设备名称前缀,默认null
                //Opt.NamePrefix = "BMAU";
                //可选设置服务UUID/ServiceUUID,默认0xff00.[非空!]
                //Opt.ServiceUuid = "e7810a71-73ae-499d-8c15-faa9aef0c3f2";
                //Opt.FiltersServices =new object[] { 0xff00, 0xfee7, "e7810a71-73ae-499d-8c15-faa9aef0c3f2" };
                Opt.ServiceUuid = Opt.ServiceUuid ?? 0xff00;
                await module.InvokeVoidAsync("printFunction", InstancePrinter, PrinterElement,Opt);
            }
        }
        catch (Exception e)
        {
            if (OnError != null) await OnError.Invoke(e.Message);
        }
    }

    /// <summary>
    /// 打印
    /// </summary>
    public virtual async Task Print()
    {
        try
        {
            await module!.InvokeVoidAsync("printFunction", InstancePrinter, PrinterElement, Opt, "write", Commands);
        }
        catch (Exception e)
        {
            if (OnError != null) await OnError.Invoke(e.Message);
        }
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        InstancePrinter?.Dispose();
        if (module is not null)
        {
            await module.DisposeAsync();
        }
    }

    /// <summary>
    /// 连接完成回调方法
    /// </summary>
    /// <param name="opt"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    [JSInvokable]
    public async Task GetResult(PrinterOption opt,string status)
    {
        try
        {
            Opt = opt;
            if (OnResult != null) await OnResult.Invoke($"{opt.Devicename}{status}");
        }
        catch (Exception e)
        {
            if (OnError != null) await OnError.Invoke(e.Message);
        }
    }

    /// <summary>
    /// 获得/设置 连接完成回调方法
    /// </summary>
    [Parameter]
    public Func<string, Task>? OnResult { get; set; }

    /// <summary>
    /// 获取已配对设备回调方法
    /// </summary>
    /// <param name="devices"></param>
    /// <returns></returns>
    [JSInvokable]
    public async Task GetDevices(List<string>? devices)
    {
        try
        {
            Devices = devices;
            if (OnGetDevices != null) await OnGetDevices.Invoke(devices);
        }
        catch (Exception e)
        {
            if (OnError != null) await OnError.Invoke(e.Message);
        }
    }

    /// <summary>
    /// 获得/设置 获取已配对设备回调方法
    /// </summary>
    [Parameter]
    public Func<List<string>?, Task>? OnGetDevices { get; set; }

    /// <summary>
    /// 连接指定已配对设备
    /// </summary>
    public virtual async Task ConnectDevices(string? devicename=null)
    {
        try
        {
            if (devicename!=null) Opt.Devicename = devicename;
            await module!.InvokeVoidAsync("connectdevice", InstancePrinter, PrinterElement, Opt, Commands);
        }
        catch (Exception e)
        {
            if (OnError != null) await OnError.Invoke(e.Message);
        }
    }

    /// <summary>
    /// 获得/设置 错误回调方法
    /// </summary>
    [Parameter]
    public Func<string, Task>? OnError { get; set; }
 
    /// <summary>
    /// 状态更新回调方法
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    [JSInvokable]
    public async Task UpdateStatus(string status)
    {
        if (OnUpdateStatus != null) await OnUpdateStatus.Invoke(status);
    }

    /// <summary>
    /// 错误更新回调方法
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    [JSInvokable]
    public async Task UpdateError(string status)
    {
        if (OnUpdateError != null) await OnUpdateError.Invoke(status);
    }

}
