// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

namespace BlazorHybrid.Shared;

public partial class DataService
{

    /// <summary>
    /// 初始化环境
    /// </summary>
    /// <returns></returns>
    public void InitDatas()
    {
        if (Fsql.Select<SysInfo>().Count() == 0)
        {
            SyncStructure();

            if (Fsql.Insert<SysInfo>().AppendData(SysInfo.InitDatas()).ExecuteAffrows() == 1)
            {
                Log("系统初始化");

                SysInfo = Fsql.Select<SysInfo>().First();

                if (Fsql.Select<Users>().Count() < 1)
                {
                    var itemList = Users.GenerateDatas(Hasher, SysInfo.Salt);
                    Fsql.Insert<Users>().AppendData(itemList).ExecuteAffrows();

                    InitDemoProjects();
                    Log("系统初始化成功");
                }

            }
        }
        else
        {
            SysInfo = Fsql.Select<SysInfo>().First();
        }
    }

    public string? SyncStructure()
    {
        try
        {
            Fsql.CodeFirst.SyncStructure<Photos>();
            Fsql.CodeFirst.SyncStructure<SysLog>();
            Fsql.CodeFirst.SyncStructure<Users>();
            Fsql.CodeFirst.SyncStructure<SysInfo>();
            Log($"建立演示数据成功");
            return null;
        }
        catch (Exception e)
        {
            Log($"建立演示数据失败,{e.Message}");
            return e.Message;
        }
    }

    public void Log(string message)
    {
        try
        {
            Fsql.Insert<SysLog>().AppendData(new SysLog { Message = message }).ExecuteAffrows();
        }
        catch { }
    }

    public List<Users> GetUsers() => Fsql.Select<Users>().ToList();

    public bool InitDemoProjects(bool rebuild = false)
    {


        return false;

    }
}