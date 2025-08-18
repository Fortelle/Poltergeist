using System.Text.Json;

namespace Poltergeist.Automations.Utilities.Cryptology;

public static class SerializationUtil
{
    public static T? JsonLoad<T>(string path, JsonSerializerOptions? options = null)
    {
        using var fs = File.Open(path, FileMode.Open, FileAccess.Read);
        var obj = JsonSerializer.Deserialize<T>(fs, options);
        return obj;
    }

    public static void JsonSave<T>(string path, T obj, JsonSerializerOptions? options = null)
    {
        var text = JsonSerializer.Serialize(obj, options);

        var folder = Path.GetDirectoryName(path);
        if (folder is not null && !Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        File.WriteAllText(path, text);
    }

}
