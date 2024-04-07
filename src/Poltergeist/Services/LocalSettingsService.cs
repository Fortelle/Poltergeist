using Poltergeist.Automations.Parameters;

namespace Poltergeist.Services;

public class LocalSettingsService
{
    public ParameterDefinitionValueCollection Settings = new();
    public event Action<string, object>? Changed;

    public LocalSettingsService()
    {
    }

    public void Add(IParameterDefinition definition)
    {
        Settings.Add(definition);
    }

    public void Load()
    {
        var filepath = App.GetService<PathService>().LocalSettingsFile;
        Settings.Load(filepath);
    }

    public T? Get<T>(string key)
    {
        return Settings.Get<T>(key);
    }

    public void Set<T>(string key, T value)
    {
        Settings.Set(key, value);
    }

    public void Save()
    {
        Settings.Save();
    }

    public void OnChanged(string key, object value)
    {
        Changed?.Invoke(key, value);
    }
}
