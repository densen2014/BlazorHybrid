using BootstrapBlazor.WebAPI.Services;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace bh002_ORM.Pages;

public partial class Index
{
    [Inject, NotNull] protected IStorage Storage { get; set; }

    [DisplayName("用户名")]
    [Required(ErrorMessage = "请输入用户名")]
    public string Username { get; set; }

    IFreeSql Fsql { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Username = await Storage.GetValue("username","");
            if (!string.IsNullOrEmpty(Username))
            {
                StateHasChanged();
            }
        }
    }

    async Task Login()
    {
        await Storage.SetValue("username", Username);
    }

    async Task OrmTestAsync()
    {
        Fsql = new FreeSql.FreeSqlBuilder()
             .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=freedb.db")
             .UseMonitorCommand(cmd => Console.WriteLine($"Sql：{cmd.CommandText}"))//监听SQL语句
             .UseAutoSyncStructure(true) //自动同步实体结构到数据库，FreeSql不会扫描程序集，只有CRUD时才会生成表。
             .Build();
    }
}


