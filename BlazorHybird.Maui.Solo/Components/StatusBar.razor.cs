// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorHybrid.Components;

public partial class StatusBar
{
    UsersModifyPassword passwordModel = new UsersModifyPassword();

    protected Modal? ExtraLargeModal { get; set; }
    bool IsShow { get; set; } = false;

    string? GetFullName()
    {
        var info = States.User!.FullName;

        if (States.User!.Type == UserType.试用用户)
        {
            info += "(试用)";
        }

        return info;
    }

    string LoginInfo()
    {
        var info = $"{States.User!.Username}({States.User!.Type})";

        if (States.User!.Type == UserType.试用用户)
        {
            info = $"{States.User.Username}(剩余次数:{States.User.TrialTimes})";
        }

        return info;
    }

    private async Task Logout()
    {
        await Storage.RemoveValue("hash");
        States.IsLoging = false;
        await Changed.InvokeAsync();

        //注销这句就会刷新布局
        NavigationManager.NavigateTo("/", false);
    }

    private Task Features()
    {
        NavigationManager.NavigateTo("PlatformFeatures");
        return Task.CompletedTask;
    }

    private async Task Personal()
    {
        IsShow = !IsShow;
        if (IsShow)
        {
            await ExtraLargeModal!.Show();
        }
    }

    private Task OnCloseAsync()
    {
        IsShow = false;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task OnValidSubmit(EditContext context)
    {
        await Save();
    }

    private async Task OnValidChangePasswordSubmit(EditContext context)
    {
        if (States.User == null || passwordModel?.Password == null)
        {
            return;
        }

        var res = DataService.ChangePassword(States.User.UserID, passwordModel.Password);
        if (res.users != null)
        {
            States.User = res.users;

            await ToastService.Success("提示", res.message);

            StateHasChanged();
        }
        else
        {
            await ToastService.Error("提示", res.message);
            States.IsLoging = false;
        }
    }

    async Task Save()
    {
        if (States.User == null)
        {
            return;
        }

        var res = DataService.SaveUser(States.User);
        if (res.users != null)
        {
            States.User = res.users;

            await ToastService.Success("提示", res.message);
        }
        else
        {
            await ToastService.Error("保存失败! ", res.message);
            States.IsLoging = false;
        }
    }

}


