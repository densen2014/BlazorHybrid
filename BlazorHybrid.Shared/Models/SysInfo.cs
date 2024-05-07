// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BootstrapBlazor.Components;
using FreeSql.DataAnnotations;
using System.ComponentModel;

namespace BlazorHybrid.Shared;


/// <summary>
/// 系统表
/// </summary>
public class SysInfo
{

    [AutoGenerateColumn(Visible = false, Editable = false)]
    [Column(IsIdentity = true, IsPrimary = true)]
    [DisplayName("序号")]
    public Guid UserID { get; set; }

    [DisplayName("软件名称")]
    public string? AppName { get; set; }

    [DisplayName("账号注册后默认启用")]
    public bool AutoEnable { get; set; }

    [DisplayName("强制使用本机功能")]
    public bool ForceNativeFunction { get; set; }

    [DisplayName("账号试用次数")]
    public long? MaxTrialTimes { get; set; } = 50;

    [DisplayName("密码盐")]
    public string? Salt { get; set; }

    [DisplayName("版本")]
    public int? Ver { get; set; } = 1;

    public static SysInfo InitDatas()
    {
        var Item = new SysInfo
        {
            AppName = "演示 BlazorHybrid",
            Salt = "vG9L7i3dtQac"
        };

        return Item;
    }
}
