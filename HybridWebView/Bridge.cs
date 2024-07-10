// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using System.Runtime.InteropServices;

#nullable enable

namespace BlazorHybrid.Win.Shared;

public class BridgeObject
{
    public string MacAdress { get; set; } = Guid.NewGuid().ToString();
}

public partial class LocalService
{

    public static BridgeObject obj = new BridgeObject(); 

    /// <summary>
    /// 自定义宿主类，用于向网页注册C#对象，供JS调用
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class Bridge
    {
        public string Func(string param) => $"Func返回 {param} {obj.MacAdress}"; 

    }

 }

