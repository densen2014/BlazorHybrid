// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

namespace BlazorHybrid.Shared;

/// <summary>
/// 用户
/// </summary>
public partial class DataService
{

    public (Users? users, string? message) Login(string name, string password, string? hash = null)
    {
        password = hash ?? Hasher.HashPassword(password, SysInfo?.Salt);
        var maxTrialTimes = SysInfo?.MaxTrialTimes ?? 50;
        var user = Fsql.Select<Users>().Where(a => a.Username == name && a.Password == password && (a.TrialTimes ?? maxTrialTimes) > 0).First();
        if (user != null)
        {
            if (!(user.Enable ?? false))
            {
                return (null, (user.LoginTimes ?? 0) == 0 ? "用户注册后未激活,请联系系统管理员." : "用户已停用,请联系系统管理员.");
            }
            user.LoginTimes = (user.LoginTimes ?? 0) + 1;
            if (user.Type == UserType.试用用户)
            {
                user.TrialTimes = (user.TrialTimes ?? maxTrialTimes) - 1;
            }
            Fsql.Update<Users>().SetSource(user).ExecuteAffrows();
            return (user, null);
        }
        else
        {
            return (null, "请检查用户名或密码.");
        }
    }

    public (Users? users, string? message) Register(Users newuser)
    {
        var user = Fsql.Select<Users>().Where(a => a.Username == newuser.Username).First();
        if (user == null)
        {
            newuser.Type = UserType.游客;
            newuser.Enable = false;
            newuser.Password = Hasher.HashPassword(newuser.Password!, SysInfo?.Salt);
            var res = Fsql.Insert(newuser).ExecuteAffrows();
            if (res == 1)
            {
                return (Fsql.Select<Users>().Where(a => a.Username == newuser.Username).First(), null);
            }
        }
        return (null, "用户名已存在,请重新注册其他用户名.");
    }

    public (Users? users, string? message) SaveUser(Users userModify)
    {
        var user = Fsql.Select<Users>().Where(a => a.Username == userModify.Username && a.UserID != userModify.UserID).First();
        if (user == null)
        {
            var res = Fsql.Update<Users>().SetSource(userModify).ExecuteAffrows();
            if (res == 1)
            {
                return (Fsql.Select<Users>().Where(a => a.Username == userModify.Username).First(), "资料保存成功");
            }
        }
        return (null, "用户名已存在,请重输入用户名.");
    }

    public (Users? users, string? message) ChangePassword(Guid userID, string password)
    {
        password = Hasher.HashPassword(password, SysInfo?.Salt);
        var res = Fsql.Update<Users>().Set(a => a.Password == password).Where(a => a.UserID == userID).ExecuteAffrows();
        if (res == 1)
        {
            return (Fsql.Select<Users>().Where(a => a.UserID == userID).First(), "密码修改成功");
        }
        else
        {
            return (null, "密码修改失败,请重试.");
        }
    }

}