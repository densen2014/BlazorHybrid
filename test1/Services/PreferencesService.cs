// ********************************** 
// Densen Informatica 中讯科技 
// 作者：Alex Chow
// e-mail:zhouchuanglin@gmail.com 
// **********************************

namespace BootstrapBlazor.WebAPI.Services;



public class PreferencesService : IStorage
{

    public PreferencesService()
    {
    }

    public Task<TValue?> GetValue<TValue>(string key, TValue? def) => Task.FromResult(Preferences.Default.Get(key, def));

    public Task SetValue<TValue>(string key, TValue value)
    {
        Preferences.Default.Set(key, value);
        return Task.CompletedTask;
    }

    public Task RemoveValue(string key)
    {
        Preferences.Default.Remove(key);
        return Task.CompletedTask;
    }

}



