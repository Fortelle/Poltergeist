using System.Collections.Generic;
using Poltergeist.Common.Utilities.Cryptology;

namespace Poltergeist.Services;

public class LocalSettingsService
{
    private readonly PathService PathService;

    private Dictionary<string, object> Settings;

    public LocalSettingsService(PathService pathService)
    {
        PathService = pathService;

        var filepath = pathService.LocalSettingsFile;
        SerializationUtil.JsonLoad(filepath, out Settings);
        Settings ??= new();
    }

    public T ReadSetting<T>(string key, T def = default)
    {
        if (Settings.TryGetValue(key, out var obj))
        {
            SerializationUtil.JsonDeserialize((string)obj, out def);
        }

        return def;
    }

    public void SaveSetting<T>(string key, T value)
    {
        Settings[key] = SerializationUtil.JsonStringify(value);

        var filepath = PathService.LocalSettingsFile;
        SerializationUtil.JsonSave(filepath, Settings);
    }
}
