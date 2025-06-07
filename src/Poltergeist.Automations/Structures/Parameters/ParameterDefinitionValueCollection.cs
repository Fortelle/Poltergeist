using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Poltergeist.Automations.Structures.Parameters;

/// <summary>
/// Represents a collection of parameter definitions and their associated values.
/// </summary>
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

    private readonly ConcurrentDictionary<string, IParameterDefinition> DefinitionCollection = new();
    private readonly ConcurrentDictionary<string, object?> ValueCollection = new();
    private readonly ConcurrentDictionary<string, JsonNode> JsonCollection = new();

    private string? FilePath;
    private bool HasChanged;

    public ParameterDefinitionValueCollection()
    {
    }

    public ParameterDefinitionValueCollection(ParameterDefinitionCollection definitions)
    {
        foreach (var definition in definitions)
        {
            DefinitionCollection.TryAdd(definition.Key, definition);
        }
    }

    public void AddDefinition(IParameterDefinition definition)
    {
        DefinitionCollection.TryAdd(definition.Key, definition);
    }

    public bool ContainsDefinition(string key)
    {
        return DefinitionCollection.ContainsKey(key);
    }

    public void Remove(string key)
    {
        HasChanged |= DefinitionCollection.TryRemove(key, out _);
        HasChanged |= ValueCollection.TryRemove(key, out _);
        HasChanged |= JsonCollection.TryRemove(key, out _);
    }

    public object? Get(string key)
    {
        DefinitionCollection.TryGetValue(key, out var definition);
        ValueCollection.TryGetValue(key, out var value);

        if (definition is null)
        {
            return value;
        }

        value ??= GetJsonValue(key, definition.BaseType);

        value ??= definition?.DefaultValue;

        return value;
    }

    public T? Get<T>(string key)
    {
        DefinitionCollection.TryGetValue(key, out var definition);
        ValueCollection.TryGetValue(key, out var value);

        value ??= GetJsonValue(key, definition?.BaseType ?? typeof(T));

        value ??= definition?.DefaultValue ?? default;

        return (T?)value;
    }

    public void Set(string key, object? value)
    {
        SetInternal(key, value);

        HasChanged = true;
    }

    private object? GetJsonValue(string key, Type valueType)
    {
        if (JsonCollection.TryGetValue(key, out var jsonNode))
        {
            try
            {
                var value = JsonSerializer.Deserialize(jsonNode, valueType, SerializerOptions);
                SetInternal(key, value);
                JsonCollection.TryRemove(key, out _);
                return value;
            }
            catch
            {
            }
        }

        return null;
    }

    private void SetInternal(string key, object? value)
    {
        if (DefinitionCollection.TryGetValue(key, out var definition))
        {
            if (definition.DefaultValue == value)
            {
                ValueCollection.TryRemove(key, out _);
            }
            else
            {
                ValueCollection[key] = value;
            }
        }
        else if (value is null)
        {
            ValueCollection.TryRemove(key, out _);
        }
        else
        {
            ValueCollection[key] = value;
        }
    }

    public void Load(string path)
    {
        FilePath = path;

        try
        {
            var text = File.ReadAllText(FilePath);
            var json = JsonNode.Parse(text);
            if (json is null)
            {
                return;
            }
            foreach (var (key, jsonNode) in json.AsObject())
            {
                if (jsonNode is not null)
                {
                    JsonCollection[key] = jsonNode;
                }
            }
        }
        catch
        {
        }
    }

    public void Save()
    {
        if (!HasChanged)
        {
            return;
        }

        if (string.IsNullOrEmpty(FilePath))
        {
            return;
        }

        SaveAs(FilePath);

        HasChanged = false;
    }

    public void SaveAs(string path)
    {
        var dict = new Dictionary<string, object>();
        foreach (var (key, value) in ValueCollection)
        {
            if (value is not null)
            {
                dict.Add(key, value);
            }
        }
        foreach (var (key, value) in JsonCollection)
        {
            if (value is not null && !dict.ContainsKey(key))
            {
                dict.Add(key, value);
            }
        }

        var text = JsonSerializer.Serialize(dict, SerializerOptions);

        var folder = Path.GetDirectoryName(path);
        if (folder is not null && !Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        File.WriteAllText(path, text);
    }

    public ParameterDefinitionValuePair[] GetDefinitionValueCollection()
    {
        var list = new List<ParameterDefinitionValuePair>();
        foreach (var definition in DefinitionCollection.Values.OrderBy(x => x.Key))
        {
            list.Add(new(definition, Get(definition.Key)));
        }

        return list.ToArray();
    }

    public Dictionary<string, object?> GetValueDictionary(bool includesOrphans = false)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var definition in DefinitionCollection.Values)
        {
            dict.Add(definition.Key, Get(definition.Key));
        }

        if (includesOrphans)
        {
            foreach (var entry in ValueCollection.Where(x => !dict.ContainsKey(x.Key)))
            {
                dict.Add(entry.Key, entry.Value);
            }

            foreach (var entry in JsonCollection.Where(x => !dict.ContainsKey(x.Key)))
            {
                dict.Add(entry.Key, entry.Value);
            }
        }

        return dict;
    }
}
