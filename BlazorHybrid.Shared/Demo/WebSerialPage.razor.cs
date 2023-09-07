// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************


using BootstrapBlazor.Components;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace BlazorHybrid.Shared.Pages;

public partial class WebSerialPage
{

    private string? message;
    private string? statusmessage;
    private string? errmessage;
    private WebSerialOptions options = new WebSerialOptions() { BaudRate = 115200 };

    [NotNull]
    private IEnumerable<SelectedItem> BaudRateList { get; set; } = ListToSelectedItem();

    [DisplayName("波特率")]
    private int SelectedBaudRate { get; set; } = 115200;
    private bool Flag { get; set; } 
    private bool IsConnected { get; set; } 
    private WebSerial? WebSerial { get; set; } 

 
    private Task OnReceive(string? message)
    {
        this.message = $"{DateTime.Now:hh:mm:ss} 收到数据: {message}{Environment.NewLine}"+ this.message;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task OnConnect(bool flag)
    {
        this.IsConnected = flag;
        if (flag) {
            message = null;
            statusmessage = null;
            errmessage = null;
        }
        else
        {
            //Flag=false;
        }
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task OnLog(string message)
    {
        this.statusmessage = message;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task OnError(string message)
    {
        this.errmessage = message;
        StateHasChanged();
        return Task.CompletedTask;
    }

    public static IEnumerable<SelectedItem> ListToSelectedItem()
    {
        foreach (var item in WebSerialOptions.BaudRateList)
        {
            yield return new SelectedItem(item.ToString(), item.ToString());
        }
    }
    private Task OnApply()
    {
        options.BaudRate = SelectedBaudRate;
        Flag = !Flag;
        //StateHasChanged();
        return Task.CompletedTask;
    }

     
}
