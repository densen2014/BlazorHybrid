using BootstrapBlazor.WebAPI.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using FreeSql.DataAnnotations;
#if WINDOWS
using Windows.Storage;
#endif 

namespace bh002_ORM.Pages;

public partial class Index
{
    [Inject, NotNull] protected IStorage Storage { get; set; }

    [DisplayName("用户名")]
    [Required(ErrorMessage = "请输入用户名")]
    string Username { get; set; }

    IFreeSql Fsql { get; set; } 

    [DisplayName("用户名")]
    [Required(ErrorMessage = "请输入用户名")]
    public string NewUsername { get; set; }

    List<Users> UserList { get; set; }

    string Dbpath { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Username = await Storage.GetValue("username","");
            if (!string.IsNullOrEmpty(Username))
            {
                StateHasChanged();
            }

            await OrmTestAsync();
        }
    }

    async Task Login()
    {
        await Storage.SetValue("username", Username);
    }

    async Task Reset()
    {
        await Storage.RemoveValue("username");
        Username = "";
    }

    async Task OrmTestAsync()
    {
#if WINDOWS
        string dbpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "hybrid.db");
#elif ANDROID || IOS || MACCATALYST
        string dbpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "hybrid.db");
#else
        string dbpath = "hybrid.db";
#endif 

        Fsql = new FreeSql.FreeSqlBuilder()
             .UseConnectionString(FreeSql.DataType.Sqlite, $"Data Source={dbpath};")
             //调试sql语句输出
             .UseMonitorCommand(cmd => System.Console.WriteLine(cmd.CommandText + Environment.NewLine))
             //自动同步实体结构到数据库，FreeSql不会扫描程序集，只有CRUD时才会生成表。
             .UseAutoSyncStructure(true)
            .UseNoneCommandParameter(true)
            .Build();

        if (Fsql.Select<Users>().Count() < 1)
        {
            var itemList = Users.GenerateDatas();
            Fsql.Insert<Users>().AppendData(itemList).ExecuteAffrows();
        }

        UserList= await Fsql.Select<Users>().ToListAsync();

        Dbpath = dbpath;

        StateHasChanged();

    }

    async Task Add()
    {
        Fsql.Insert(new Users() { Username = NewUsername , FullName = "试用用户"}).ExecuteAffrows();

        UserList = await Fsql.Select<Users>().ToListAsync();

        StateHasChanged();
    }

}


/// <summary>
/// 用户表
/// </summary>
public class Users
{

    [Column(IsIdentity = true, IsPrimary = true)]
    [DisplayName("序号")]
    public Guid UserID { get; set; }

    //[Column(IsPrimary = true)]
    [DisplayName("用户名")]
    [Required]
    public string Username { get; set; }

    [DisplayName("真实姓名")]
    [Required]
    public string FullName { get; set; }

    [DisplayName("启用")]
    public bool? Enable { get; set; } = false;

    public static List<Users> GenerateDatas()
    {
        var ItemList = new List<Users>()
        {
            new Users {
                Username = "root" ,
                FullName="超级用户",
                Enable =false
                },
            new Users {
                Username = "admin" ,
                FullName="管理员",
                Enable =true
                },
            new Users {
                Username = "guest" ,
                FullName="游客",
                Enable =false
                },
            new Users {
                Username = "user" ,
                FullName="正式用户",
                Enable =true
                },
        };

        return ItemList;
    }
}

