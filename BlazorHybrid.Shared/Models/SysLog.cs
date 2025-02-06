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
/// 日志表
/// </summary>
public class SysLog
{

    [AutoGenerateColumn(IsReadonlyWhenEdit = true,IsReadonlyWhenAdd = true, DefaultSort = true, DefaultSortOrder = SortOrder.Desc)]
    [Column(IsIdentity = true, IsPrimary = true)]
    [DisplayName("序号")]
    public int ID { get; set; }

    [AutoGenerateColumn(FormatString = "yyyy-MM-dd HH:mm:ss", IsReadonlyWhenAdd = true, IsReadonlyWhenEdit = true)]
    [DisplayName("日期")]
    public DateTime? Date { get; set; } = DateTime.Now;

    [AutoGenerateColumn(ShowTips = true, TextEllipsis = true, ComponentType = typeof(Textarea))]
    [Column(StringLength = -2)]
    [DisplayName("日志")]
    public string? Message { get; set; }


    /// <summary>
    /// 自动填写,第一次建立就填写登录的用户名
    /// </summary>
    [AutoGenerateColumn(Visible = false, IsReadonlyWhenEdit = true,IsReadonlyWhenAdd = true)]
    [DisplayName("经手人")]
    public Guid? Operator { get; set; }

}