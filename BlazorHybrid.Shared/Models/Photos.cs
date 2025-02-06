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
/// 照片表
/// </summary>
[AutoGenerateClass(Searchable = true, Filterable = true, Sortable = true, ShowTips = true)]
public class Photos
{

    [AutoGenerateColumn(Visible = false, IsReadonlyWhenEdit = true,IsReadonlyWhenAdd = true)]
    [Column(IsIdentity = true, IsPrimary = true)]
    public Guid? PhotoID { get; set; }

    [DisplayName("所属工程代号")]
    public string? ProjectID { get; set; }

    [DisplayName("照片")]
    [Column(StringLength = -2)]
    public string? PhotoPath { get; set; }

    /// <summary>
    /// 自动填写,第一次建立就填写登录的用户名
    /// </summary>
    [AutoGenerateColumn(Visible = false, IsReadonlyWhenEdit = true,IsReadonlyWhenAdd = true)]
    [DisplayName("拍摄人")]
    public Guid? Operator { get; set; }

    /// <summary>
    /// 自动填写,第一次建立就填写登录的用户名
    /// </summary>
    [AutoGenerateColumn(IsReadonlyWhenEdit = true,IsReadonlyWhenAdd = true)]
    [Column(IsIgnore = true)]
    [DisplayName("创建人")]
    public string? OperatorName { get => Userss?.FullName; }

    [DisplayName("用户表")]
    [AutoGenerateColumn(Ignore = true)]
    [Navigate(nameof(Operator))]
    public virtual Users? Userss { get; set; }

    [AutoGenerateColumn(FormatString = "yyyy-MM-dd")]
    [DisplayName("拍摄日期")]
    public DateTime? Date { get; set; } = DateTime.Now;

    ///// <summary>
    ///// 位置型,自动填写
    ///// </summary>
    //[DisplayName("拍摄地点")]
    //public string? Location { get; set; }

    /// <summary>
    /// 经度,自动填写
    /// </summary>
    [DisplayName("经度")]
    [AutoGenerateColumn(FormatString = "N4")]
    public decimal? Longitude { get; set; }

    /// <summary>
    /// 纬度,自动填写
    /// </summary>
    [DisplayName("纬度")]
    [AutoGenerateColumn(FormatString = "N4")]
    public decimal? Latitude { get; set; }


}

