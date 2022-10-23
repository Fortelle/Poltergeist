using System;
using System.Collections.Generic;
using System.Drawing;
using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Logging;

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
        // Application
        yield return new OptionItem<int>("app.maxrecentmacros", 10)
        {
            Category = "Application",
            DisplayLabel = "Max recent macros",
        };

        yield return new OptionItem<string[]>("app.recentmacros", Array.Empty<string>())
        {
            IsBrowsable = false,
        };

        yield return new OptionItem<Rectangle>("app.windowposition")
        {
            IsBrowsable = false,
        };

        // Macro
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

    public T GetSetting<T>(string key, T def = default)
    {
        return Settings.TryGet<T>(key, def);
    }

    public void SetSetting<T>(string key, T value)
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
