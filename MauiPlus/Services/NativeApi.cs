// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

using System.Text;
using System.Text.Json;

namespace MauiPlus;

internal partial class NativeApi : object
{
    private string PrinterNameKey = "PrinterName";
    private string printerName = "Unknown";

    public Task<string> set_config(string printerName)
    {
        Preferences.Set(PrinterNameKey, printerName);
        return Task.FromResult("ok");
    }

    /// <summary>
    /// 从应用程序的首选项中获取打印机名称 (printerName)
    /// </summary>
    /// <returns></returns>
    public Task<string> get_config()
    {
        printerName = Preferences.Default.Get(PrinterNameKey, printerName);
        return Task.FromResult(printerName);
    }

    /// <summary>
    /// 打开文件选择对话框,读取文件内容并将其转换为 Base64 编码的字符串返回
    /// </summary>
    /// <returns>文件内容 Base64 编码的字符串</returns>
    public async Task<string> open_file_dialog()
    {
        //work in ui thread
        var res =
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(new PickOptions());
                if (result == null)
                {
                    return "";
                }
                using var stream = await result.OpenReadAsync();
                StreamReader reader = new StreamReader(stream);
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(reader.ReadToEnd()));
            }
            catch (Exception e)
            {
                var err = e.Message;
                return err;
            }
        });
        return res;
    }

    /// <summary>
    /// 将给定的数据保存到指定文件名的文件,返回文件路径
    /// </summary>
    /// <param name="data"></param>
    /// <param name="fileName"></param>
    /// <returns>文件路径</returns>
    public async Task<string> save_file(string data, string fileName)
    {
        try
        {
            string targetFile = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, fileName);

            using FileStream outputStream = File.OpenWrite(targetFile);
            using StreamWriter streamWriter = new(outputStream);

            await streamWriter.WriteAsync(data);
            return $"file path:{targetFile.Replace("\\", "\\\\")}";
        }
        catch (Exception e)
        {
            var err = e.Message;
            return err;
        }
    }

    /// <summary>
    /// 存储自定义对象 User, 将自定义对象序列化为 string 类型，然后再存储
    /// </summary>
    /// <param name="user"></param>
    public void SaveUser(User user)
    {
        string jsonString = JsonSerializer.Serialize(user);
        Preferences.Set("user", jsonString);
    }

    /// <summary>
    /// 检索自定义对象 User, 从 Preferences 中检索字符串，然后将其反序列化为自定义对象
    /// </summary>
    /// <returns></returns>
    public User? GetUser()
    {
        string jsonString = Preferences.Get("user", string.Empty);
        if (string.IsNullOrEmpty(jsonString))
        {
            return null;
        }
        return JsonSerializer.Deserialize<User>(jsonString);
    }

}

/// <summary>
/// 在 Preferences 中存储自定义对象, https://www.cnblogs.com/densen2014/p/18710319
/// </summary>
public class User
{
    public string? Name { get; set; }
    public int Age { get; set; }
}
