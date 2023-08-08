// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;

namespace BlazorHybrid.Core.Device;

/// <summary>
/// 蓝牙设备 BT BatteryLevel 组件
/// </summary>
public partial class BatteryLevel : IAsyncDisposable
{

    /// <summary>
    /// UI界面元素的引用对象
    /// </summary>
    public ElementReference BatteryLevelElement { get; set; }

    /// <summary>
    /// 获得/设置 蓝牙设备
    /// </summary>
    [Parameter]
    public BluetoothDevice? Device { get; set; } = new BluetoothDevice();

    /// <summary>
    /// 获得/设置 状态更新回调方法
    /// </summary>
    [Parameter]
    public Func<BluetoothDevice, Task>? OnUpdateStatus { get; set; }

    /// <summary>
    /// 获得/设置 数值更新回调方法
    /// </summary>
    [Parameter]
    public Func<decimal, Task>? OnUpdateValue { get; set; }

    /// <summary>
    /// 获得/设置 错误更新回调方法
    /// </summary>
    [Parameter]
    public Func<string, Task>? OnUpdateError { get; set; }

    /// <summary>
    /// 获得/设置 显示log
    /// </summary>
    [Parameter]
    public bool Debug { get; set; }

    bool IsInit { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            if (firstRender)
            {
                await Init();
            }
        }
        catch (Exception e)
        {
            if (OnUpdateError != null) await OnUpdateError.Invoke(e.Message);
        }
    }

    async Task Init()
    {
        if (IsInit) return;

        if (await Tools.BluetoothIsBusy())
        {
            await ToastService.Warning("蓝牙正在使用中，请稍后再试");
            return;
        }

        Device ??= new BluetoothDevice();
        Tools.UpdateDevicename += UpdateDevicename;
        Tools.UpdateValue += UpdateValue;
        Tools.OnMessage += UpdateStatus;
        Tools.OnDataReceived += UpdateStatus;
        Tools.UpdateStatus += UpdateStatus;
        Tools.UpdateError += UpdateError;
        Tools.SetTagDeviceName(new BleTagDevice() { ScanTimeout = 30 });
        IsInit = true;
    }

    /// <summary>
    /// 查询电量
    /// </summary>
    public virtual async Task GetBatteryLevel()
    {
        try
        {
            if (await Tools.BluetoothIsBusy())
            {
                await ToastService.Warning("蓝牙正在使用中，请稍后再试");
                return;
            }
            await Init();
            await Tools.GetBatteryLevel();
        }
        catch (Exception e)
        {
            if (OnUpdateError != null) await OnUpdateError.Invoke(e.Message);
        }
    }

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        Tools.UpdateDevicename -= UpdateDevicename;
        Tools.UpdateValue -= UpdateValue;
        Tools.OnMessage -= UpdateStatus;
        Tools.OnDataReceived -= UpdateStatus;
        Tools.UpdateStatus -= UpdateStatus;
        Tools.UpdateError -= UpdateError;
        return new ValueTask();
    }

    /// <summary>
    /// 设备名称回调方法
    /// </summary>
    /// <param name="devicename"></param>
    /// <returns></returns>
    public async void UpdateDevicename(string devicename)
    {
        Device!.Name = devicename;
        if (OnUpdateStatus != null) await OnUpdateStatus.Invoke(Device);
    }

    /// <summary>
    /// 设备电量%回调方法
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public async void UpdateValue(object obj)
    {
        var value = 0m;
        if (decimal.TryParse(obj.ToString(), out value))
        {
            Device!.Value = value;
            if (OnUpdateValue != null) await OnUpdateValue.Invoke(value);
        }
    }

    /// <summary>
    /// 状态更新回调方法
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public async void UpdateStatus(string status)
    {
        Device!.Status = status;
        if (OnUpdateStatus != null) await OnUpdateStatus.Invoke(Device);
    }

    /// <summary>
    /// 错误更新回调方法
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public async void UpdateError(string status)
    {
        if (OnUpdateError != null) await OnUpdateError.Invoke(status);
    }

}
