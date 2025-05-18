using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Poltergeist.Automations.Structures.Parameters;

public class ParameterDefinitionValueCollection
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        Converters = {
            new JsonStringEnumConverter()
        },
    };

    private ParameterDefinitionCollection Definitions { get; }
    private Dictionary<string, object?> Values { get; } = new();

    private string? FilePath { get; set; }
    private bool HasChanged { get; set; }

    public ParameterDefinitionValueCollection()
    {
        Definitions = new();
    }

    public ParameterDefinitionValueCollection(ParameterDefinitionCollection definitions)
    {
        Definitions = definitions;
    }

    public bool ContainsKey(string key)
    {
        return Definitions.Contains(key);
    }

    public bool ContainsValue(string key)
    {
        return Values.ContainsKey(key);
    }

    public void Add(IParameterDefinition definition)
    {
        Definitions.Add(definition);
    }

    public void Set<T>(string key, T value)
    {
        if (Definitions.TryGetValue(key, out var definition))
        {
            if (value is null && definition.DefaultValue is null)
            {
                Values.Remove(key);
            }
            else if (value!.Equals(definition.DefaultValue))
            {
                Values.Remove(key);
            }
            else
            {
                Values[key] = value;
            }
        }
        else
        {
            Values[key] = value;
        }

        HasChanged = true;
    }

    public void Set<T>(IParameterDefinition definition, T value)
    {
        Set(definition.Key, value);
    }

    public T? Get<T>(string key)
    {
        var hasDef = Definitions.TryGetValue(key, out var definition);
        var hasVal = Values.TryGetValue(key, out var value);

        if (hasDef && !hasVal)
        {
            value = definition!.DefaultValue;
        }

        try
        {
            if (value is T tvalue)
            {
                return tvalue;
            }
            else if (value is JsonNode jsonNode)
            {
                value = JsonSerializer.Deserialize<T>(jsonNode, SerializerOptions);
                Values[key] = value;
            }

            return (T?)value;
        }
        catch { }

        return default;
    }


    public object? Get(string key)
    {
        var hasDef = Definitions.TryGetValue(key, out var definition);
        var hasVal = Values.TryGetValue(key, out var value);

        try
        {
            if (hasDef)
            {
                if (hasVal)
                {
                    if (value is JsonNode jsonNode)
                    {
                        value = JsonSerializer.Deserialize(jsonNode, definition!.BaseType, SerializerOptions);
                        Values[key] = value;
                    }
                }
                else
                {
                    value = definition!.DefaultValue;
                }
            }
        }
        catch { }

        return value;
    }

    public T? Get<T>(ParameterDefinition<T> definition)
    {
        return Get<T>(definition.Key);
    }

    public object? Get(IParameterDefinition definition)
    {
        return Get(definition.Key);
    }

    public void Remove(string key)
    {
        Definitions.Remove(key);

        HasChanged = true;
    }

    public void Load(string path)
    {
        FilePath = path;

        var text = File.ReadAllText(FilePath);
        var json = JsonNode.Parse(text);
        if (json is null)
        {
            return;
        }

        foreach (var (key, jsonNode) in json.AsObject())
        {
            Values[key] = jsonNode;
        }
    }

    public void Save()
    {
        if (!HasChanged)
        {
            return;
        };

        if (string.IsNullOrEmpty(FilePath))
        {
            return;
        }

        if (Values.Count == 0)
        {
            return;
        }

        SaveAs(FilePath);
        HasChanged = false;
    }

    public void SaveAs(string path)
    {
        var text = JsonSerializer.Serialize(Values, SerializerOptions);

        var folder = Path.GetDirectoryName(path);
        if (folder is not null && !Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        File.WriteAllText(path, text);
    }

    public ParameterDefinitionValuePair[] ToDefinitionValueArray()
    {
        var list = new List<ParameterDefinitionValuePair>();
        foreach (var definition in Definitions)
        {
            list.Add(new(definition, Get(definition)));
        }

        return list.ToArray();
    }
}
