using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components.Routing;
using MenuItem = BootstrapBlazor.Components.MenuItem;

namespace BlazorHybrid.Shared.Shared;

public partial class NavMenu
{
    private IEnumerable<MenuItem> Menus { get; set; } = new List<MenuItem>
    {
            new MenuItem() { Text = "首页", Url = "/"  , Match = NavLinkMatch.All}, 
            new MenuItem() { Text = "登录", Url = "/Login"  }, 
            new MenuItem() { Text = "关于", Url = "/AboutMe" },
    };
}
