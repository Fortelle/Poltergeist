using Poltergeist.Automations.Configs;

namespace Poltergeist.Services;

public class LocalSettingsService
{
    public MacroOptions Settings = new();
    public event Action<string, object>? Changed;

    public LocalSettingsService()
    {
    }

    public void Add(IOptionItem option)
    {
        Settings.Add(option);
    }

    public void Load()
    {
        var filepath = App.GetService<PathService>().LocalSettingsFile;
        Settings.Load(filepath);
    }

    public T? Get<T>(string key, T? def = default)
    {
        return Settings.Get(key, def);
    }

    public void Set<T>(string key, T value)
    {
        Settings.Set(key, value);
    }

    public void Save()
    {
        if (!Settings.HasChanged)
        {
            return;
        }

        var filepath = App.GetService<PathService>().LocalSettingsFile;
        Settings.Save(filepath);
    }

    public void OnChanged(string key, object value)
    {
        Changed?.Invoke(key, value);
    }
}
