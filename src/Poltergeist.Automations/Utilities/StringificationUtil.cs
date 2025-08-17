using System.Collections;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Poltergeist.Automations.Utilities;

public static class StringificationUtil
{
    public static string Stringify(object? item)
    {
        return item switch
        {
            null => "(null)",
            string s => s,
            IDictionary => SerializeObject(item),
            IEnumerable ie => SerializeObject(ie),
            _ when IsToStringOverridden(item.GetType()) => $"{item}",
            _ => SerializeObject(item),
        };
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    };

    public static string SerializeObject(object? value)
    {
        try
        {
            return JsonSerializer.Serialize(value, SerializerOptions);
        }
        catch (Exception exception)
        {
            return $"<{exception.Message}>";
        }
    }

    public static bool IsToStringOverridden(Type type)
    {
        var toStringMethod = type.GetMethod("ToString", []);

        if (toStringMethod is null)
        {
            return false;
        }

        return toStringMethod.DeclaringType != typeof(object);
    }

}
