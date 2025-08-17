using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Structures;

public class SerializableParameterValueCollection : ParameterValueCollection
{
    public JsonSerializerOptions SerializerOptions { get; set; } = new()
    {
        IncludeFields = true,
        Converters =
        {
            new JsonStringEnumConverter(),
        },
    };

    public SerializableParameterValueCollection()
    {
    }

    public SerializableParameterValueCollection(IEnumerable<KeyValuePair<string, object?>> items)
    {
        foreach (var kvp in items)
        {
            Add(kvp.Key, kvp.Value);
        }
    }

    public override T Get<T>(string key)
    {
        if (!TryGetValue<T>(key, out var value))
        {
            throw new KeyNotFoundException($"The key '{key}' was not found in the dictionary.");
        }

        return value;
    }

    public override bool TryGetValue<T>(string key, [MaybeNullWhen(false)] out T value)
    {
        if (!TryGetValue(key, out var oldValue))
        {
            value = default;
            return false;
        }

        switch (oldValue)
        {
            case JsonNode jnode:
                {
                    var jvalue = jnode.Deserialize<T>(SerializerOptions);
                    Collection[key] = jvalue;
                    value = jvalue!;
                }
                break;
            case JsonElement jelement:
                {
                    var jvalue = jelement.Deserialize<T>(SerializerOptions);
                    Collection[key] = jvalue;
                    value = jvalue!;
                }
                break;
            case T tvalue:
                {
                    value = tvalue;
                }
                break;
            default:
                {
                    value = (T)oldValue!;
                }
                break;
        }

        return true;
    }

}
