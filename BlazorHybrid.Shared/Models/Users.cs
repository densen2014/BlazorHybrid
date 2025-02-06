// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using BootstrapBlazor.Components;
using FreeSql.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BlazorHybrid.Shared;


/// <summary>
/// 用户表
/// </summary>
public class Users
{

    [AutoGenerateColumn(Visible = false, IsReadonlyWhenEdit = true,IsReadonlyWhenAdd = true)]
    [Column(IsIdentity = true, IsPrimary = true)]
    [DisplayName("序号")]
    public Guid UserID { get; set; }

    //[Column(IsPrimary = true)]
    [DisplayName("用户名")]
    [Required]
    public string? Username { get; set; }

    [DisplayName("真实姓名")]
    [Required]
    public string? FullName { get; set; }

    [DisplayName("用户类型")]
    public UserType? Type { get; set; } = UserType.游客;

    [DisplayName("所属公司")]
    [Required]
    public string? Company { get; set; }

    [AutoGenerateColumn(Visible = false, IsReadonlyWhenEdit = true,IsReadonlyWhenAdd = true, ComponentType = typeof(BootstrapPassword))]
    [DisplayName("用户密码")]
    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, ErrorMessage = "'{0}'最低{2}字符,最大{1}字符.", MinimumLength = 6)]
    public string? Password { get; set; }

    [AutoGenerateColumn(Ignore = true)]
    [Column(IsIgnore = true)]
    [DataType(DataType.Password)]
    [Display(Name = "确认密码")]
    [Compare("Password", ErrorMessage = "两次密码不一致")]
    public string? ConfirmPassword { get; set; }

    [AutoGenerateColumn(Visible = false, ComponentType = typeof(BootstrapPassword))]
    [Column(IsIgnore = true)]
    [DisplayName("新密码(不更改则留空)")]
    [DataType(DataType.Password)]
    [StringLength(100, ErrorMessage = "'{0}'最低{2}字符,最大{1}字符.", MinimumLength = 6)]
    public string? NewPassword { get; set; }

    [AutoGenerateColumn(Visible = false, ComponentType = typeof(BootstrapPassword))]
    [Column(IsIgnore = true)]
    [DataType(DataType.Password)]
    [Display(Name = "确认新密码(不更改则留空)")]
    [Compare("NewPassword", ErrorMessage = "两次密码不一致")]
    public string? ConfirmNewPassword { get; set; }

    [AutoGenerateColumn(FormatString = "yyyy-MM-dd HH:mm:ss", IsReadonlyWhenAdd = true, IsReadonlyWhenEdit = true)]
    [DisplayName("创建日期")]
    public DateTime? Date { get; set; } = DateTime.Now;

    [DisplayName("启用")]
    public bool? Enable { get; set; } = false;

    [AutoGenerateColumn(Visible = false)]
    [DisplayName("试用次数")]
    public long? TrialTimes { get; set; } = 50;

    [AutoGenerateColumn(Visible = false, IsReadonlyWhenAdd = true, IsReadonlyWhenEdit = true)]
    [DisplayName("登录次数")]
    public long? LoginTimes { get; set; } = 0;

    public override string ToString() => $"[{UserID}] {Username} ({FullName})";

    public static List<Users> GenerateDatas(PasswordHasher hasher, string? sal)
    {
        var password = hasher.HashPassword("121121", sal);

        var ItemList = new List<Users>()
        {
            new Users {
                Username = "root" ,
                FullName="超级用户",
                Password =password,
                Type=UserType.超级用户,
                Company="内部",
                Enable =true
                },
            new Users {
                Username = "admin" ,
                FullName="管理员",
                Password =password,
                Type=UserType.管理员,
                Company="内部",
                Enable =true
                },
            new Users {
                Username = "guest" ,
                FullName="游客",
                Password =password,
                Type=UserType.游客,
                Enable =true
                },
            new Users {
                Username = "user" ,
                FullName="正式用户",
                Password =password,
                Type=UserType.正式用户,
                Company="亚洲重工",
                Enable =true
                },
            new Users {
                Username = "trial" ,
                FullName="试用用户",
                Password =password,
                Type=UserType.试用用户,
                Company="亚洲重工",
                Enable =true
                },
            new Users {
                Username = "user2" ,
                FullName="王光良",
                Password =password,
                Type=UserType.正式用户,
                Company="中铁十一局集团第二工程有限公司" ,
                Enable =true
                },
        };

        return ItemList;
    }
}



public class UsersModifyPassword
{
    [AutoGenerateColumn(ComponentType = typeof(BootstrapPassword))]
    [Column(IsIgnore = true)]
    [DisplayName("新密码")]
    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, ErrorMessage = "'{0}'最低{2}字符,最大{1}字符.", MinimumLength = 6)]
    public string? Password { get; set; }

    [AutoGenerateColumn(ComponentType = typeof(BootstrapPassword))]
    [Column(IsIgnore = true)]
    [DataType(DataType.Password)]
    [Display(Name = "确认密码")]
    [Compare("Password", ErrorMessage = "两次密码不一致")]
    public string? ConfirmPassword { get; set; }

}
