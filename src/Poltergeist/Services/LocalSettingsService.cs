using System.Collections.Generic;
using System.Drawing;
using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Logging;
using Poltergeist.Common.Utilities.Cryptology;

namespace Poltergeist.Services;

public class LocalSettingsService
{
    private readonly PathService PathService;

    internal MacroOptions Settings;

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

        yield return new OptionItem<LogLevel>("logger.tofile", LogLevel.All)
        {
            Category = "Logger",
            DisplayLabel = "Log to file",
        };

        yield return new OptionItem<LogLevel>("logger.toconsole", LogLevel.Information)
        {
            Category = "Logger",
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

    //public T ReadSetting<T>(string key, T def = default)
    //{
    //    if (Settings.TryGetValue(key, out var obj))
    //    {
    //        SerializationUtil.JsonDeserialize((string)obj, out def);
    //    }

    //    return def;
    //}

    //public void SaveSetting<T>(string key, T value)
    //{
    //    Settings[key] = SerializationUtil.JsonStringify(value);

    //    var filepath = PathService.LocalSettingsFile;
    //    SerializationUtil.JsonSave(filepath, Settings);
    //}
}
