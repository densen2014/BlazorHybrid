// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using Microsoft.AspNetCore.Components.Routing;
using MenuItem = BootstrapBlazor.Components.MenuItem;

namespace BlazorHybrid.Components;

public partial class NavMenu
{
    private IEnumerable<MenuItem> Menus { get; set; } = new List<MenuItem>
    {
            new MenuItem() { Text = "组件", Url = "/Components" , Icon="fa-solid fas fa-grip" },
            new MenuItem() { Text = "演示", Url = "/Index"  , Match = NavLinkMatch.All, Icon="fa-solid fas fa-horse-head"},
            new MenuItem() { Text = "关于", Url = "/About" , Icon="fa-solid fas fa-link"},
    };
}
