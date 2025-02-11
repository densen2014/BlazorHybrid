using System;
using System.Threading.Tasks;

public class NativeApi_code
{
    public string set_config()
    {
        return "ok";
    }

    public async Task<string> get_config()
    {
        await Task.Delay(1000); // 模拟异步操作
        return "123";
    }

    public async Task<string> open_file_dialog()
    {
        await Task.Delay(1000); // 模拟异步操作
        return "Hello from dynamically generated NativeApi!";
    }
    public string save_file(string content, string filename)
    {
        return "ok";
    }
}
