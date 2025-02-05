// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using AmeBlazor.Components;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BlazorHybrid.Components;

public class States
{
    [DisplayName("用户名")]
    [Required(ErrorMessage = "请输入用户名")]
    public string? Username { get; set; }

    [DisplayName("密码")]
    [Required(ErrorMessage = "请输入密码")]
    public string? Password { get; set; }
    public string? Hash { get; set; }

    [DisplayName("记住我")]
    public bool Remember { get; set; } = true;

    [DisplayName("保持登录")]
    public bool RememberPassword { get; set; }

    public bool IsLoging { get; set; } = false;

    public int Steps { get; set; } = 0;


    public Users? User { get; set; }

    public bool IsMobile { get; set; }
    public int Width { get; set; }

    public static List<TableImgField> ImgFields = new List<TableImgField>() {
        new TableImgField {
            Field = nameof(Photos.PhotoPath),
            Style= "width: 150px; height: 150px; border-radius: 10%;"
        }
    };


}
