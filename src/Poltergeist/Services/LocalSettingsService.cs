using System.Collections.Generic;
using System.Drawing;
using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Logging;
using Poltergeist.Common.Utilities.Cryptology;

namespace Poltergeist.Services;

public class LocalSettingsService
{
    public MacroOptions Settings;
    public event Action<string, object> Changed;

    private readonly PathService PathService;

    public LocalSettingsService(PathService pathService)
    {
        PathService = pathService;

        var filepath = pathService.LocalSettingsFile;
        Settings = new();
        foreach(var option in CreateSettings())
        {
            Settings.Add(option);
        }
        Settings.Load(filepath, true);
    }

    private static IEnumerable<IOptionItem> CreateSettings()
    {
        yield return new OptionItem<Rectangle>("WindowPosition")
        {
            IsBrowsable = false,
        };

        yield return new OptionItem<bool>("macro.usestatistics", true)
        {
            Category = "Macro",
            DisplayLabel = "Use statistics",
        };

        yield return new OptionItem<LogLevel>("logger.tofile", LogLevel.All)
        {
            Category = "Macro",
            DisplayLabel = "Log to file",
        };

        yield return new OptionItem<LogLevel>("logger.toconsole", LogLevel.Information)
        {
            Category = "Macro",
            DisplayLabel = "Log to console",
        };
    }

    public T ReadSetting<T>(string key, T def = default)
    {
        return Settings.TryGet<T>(key, def);
    }

    public void SaveSetting<T>(string key, T value)
    {
        Settings.Set(key, value);
    }

    public void Save()
    {
        var filepath = PathService.LocalSettingsFile;
        Settings.Save(filepath);
    }

    public void OnChanged(string key, object value)
    {
        Changed?.Invoke(key, value);
    }
}
