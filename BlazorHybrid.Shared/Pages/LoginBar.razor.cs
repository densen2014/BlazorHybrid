// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace BlazorHybrid.Shared.Pages;

public partial class LoginBar
{

    Users NewUser = new Users();

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            DataService.InitDatas();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            States.Width = await JS.InvokeAsync<int>("eval", "window.innerWidth");
            States.Username = await Storage.GetValue("username","");
            if (!string.IsNullOrEmpty(States.Username))
            {
                States.Remember = true;
                if (!string.IsNullOrEmpty(await Storage.GetValue("hash","")))
                {
                    States.RememberPassword = true;
                    States.Hash = await Storage.GetValue("hash", "");
                    await Login();
                }
                StateHasChanged();
            }
            else
            {
                States.Username = "user";
                States.Password = "121121";
                StateHasChanged();
            }
        }
    }

    private async Task OnValidSubmit(EditContext context)
    {
        await Login();
    }

    private async Task OnValidRegister(EditContext context)
    {
        var res = DataService.Register(NewUser);
        if (res.users != null)
        {
            States.Username = res.users.Username;
            States.Password = res.users.Password;
            await Login();
        }
        else
        {
            await ToastService.Error("注册失败! ", res.message);
            States.IsLoging = false;
        }

    }


    async Task Login()
    {
        if (string.IsNullOrEmpty(States.Username) || (string.IsNullOrEmpty(States.Password) && string.IsNullOrEmpty(States.Hash)))
        {
            await ToastService.Error("登录失败! ", "用户名或密码不能为空.");
            return;
        }
        var res = DataService.Login(States.Username, States.Password??"", States.Hash);
        if (res.users != null)
        {
            States.User = res.users;
            States.IsLoging = true;
            if (States.User.Type != UserType.超级用户 && States.User.Type != UserType.管理员)
            {
                States.Steps = 1;
            }

            if (States.Remember)
            {
                await Storage.SetValue("username", res.users.Username!);
                if (States.RememberPassword)
                {
                    await Storage.SetValue("hash", res.users.Password!);
                }
                else
                {
                    await Storage.RemoveValue("hash");
                }
            }
            else
            {
                await Storage.RemoveValue("username");
                await Storage.RemoveValue("hash");
            }

            await Changed.InvokeAsync();

            //注销这句就不会刷新布局.类似单页应用
            NavigationManager.NavigateTo("/Index", false);

            StateHasChanged();
        }
        else
        {
            await ToastService.Error("登录失败! ", res.message);
            States.IsLoging = false;
        }
    }


}


