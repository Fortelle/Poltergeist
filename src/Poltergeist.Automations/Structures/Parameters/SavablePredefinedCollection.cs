using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Poltergeist.Automations.Structures.Parameters;

public class SavablePredefinedCollection
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        IncludeFields = true,
        Converters = {
            new JsonStringEnumConverter()
        },
    };

    public bool AllowAbsentDefinition { get; init; }

    public bool HasChanged { get; private set; }

    public string? FilePath { get; private set; }

    public int DefinitionCount => DefinitionCollection.Count;

    private readonly Dictionary<string, IParameterDefinition> DefinitionCollection = new();
    private readonly Dictionary<string, object?> ValueCollection = new();
    private readonly Dictionary<string, JsonNode> JsonCollection = new();

    private int Hash;
    private readonly Lock _lock = new();

    public SavablePredefinedCollection()
    {
    }

    public SavablePredefinedCollection(KeyedCollection<string, IParameterDefinition> definitions)
    {
        foreach (var definition in definitions)
        {
            DefinitionCollection.TryAdd(definition.Key, definition);
        }
    }

    public void AddDefinition(IParameterDefinition definition)
    {
        lock (_lock)
        {
            DefinitionCollection.TryAdd(definition.Key, definition);
        }
    }

    public bool TryGetDefinition(string key, [MaybeNullWhen(false)] out IParameterDefinition definition)
    {
        return DefinitionCollection.TryGetValue(key, out definition);
    }

    public bool ContainsDefinition(string key)
    {
        return DefinitionCollection.ContainsKey(key);
    }

    public void Remove(string key)
    {
        lock (_lock)
        {
            HasChanged |= DefinitionCollection.Remove(key);
            HasChanged |= ValueCollection.Remove(key);
            HasChanged |= JsonCollection.Remove(key);
        }
    }

    public object? Get(string key)
    {
        if (!DefinitionCollection.TryGetValue(key, out var definition))
        {
            if (!AllowAbsentDefinition)
            {
                return default;
            }
        }
        
        ValueCollection.TryGetValue(key, out var value);

        if (definition is not null)
        {
            value ??= GetJsonValue(key, definition.BaseType);
        }

        value ??= definition?.DefaultValue;

        return value;
    }

    public T? Get<T>(string key)
    {
        if (!DefinitionCollection.TryGetValue(key, out var definition))
        {
            if (!AllowAbsentDefinition)
            {
                return default;
            }
        }

        ValueCollection.TryGetValue(key, out var value);

        value ??= GetJsonValue(key, definition?.BaseType ?? typeof(T));

        value ??= definition?.DefaultValue;

        if (value is null)
        {
            return default;
        }

        return (T?)value;
    }

    public T? Get<T>(ParameterDefinition<T> definition)
    {
        return Get<T>(definition.Key);
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
                lock (_lock)
                {
                    JsonCollection.Remove(key);
                }
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
        if (!DefinitionCollection.TryGetValue(key, out var definition))
        {
            if (!AllowAbsentDefinition)
            {
                throw new KeyNotFoundException();
            }
        }

        lock (_lock)
        {
            if (definition?.DefaultValue == value)
            {
                ValueCollection.Remove(key);
            }
            else if (value is null)
            {
                ValueCollection.Remove(key);
            }
            else
            {
                ValueCollection[key] = value;
            }
        }
    }

    public void Load(string path)
    {
        FilePath = path;

        var text = File.ReadAllText(FilePath);
        Hash = text.GetHashCode();
        var json = JsonSerializer.Deserialize<Dictionary<string, JsonNode>>(text, SerializerOptions);
        if (json is null)
        {
            return;
        }
        foreach (var (key, jsonNode) in json)
        {
            if (jsonNode is not null)
            {
                JsonCollection[key] = jsonNode;
            }
        }
    }

    public virtual bool Save()
    {
        if (!HasChanged)
        {
            return false;
        }

        if (string.IsNullOrEmpty(FilePath))
        {
            return false;
        }

        var text = GetJsonText();
        if (text.GetHashCode() == Hash)
        {
            HasChanged = false;
            return false;
        }

        var folder = Path.GetDirectoryName(FilePath);
        if (folder is not null && !Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        File.WriteAllText(FilePath, text);

        HasChanged = false;

        return true;
    }

    public virtual void SaveAs(string path)
    {
        var text = GetJsonText();

        var folder = Path.GetDirectoryName(path);
        if (folder is not null && !Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        File.WriteAllText(path, text);
    }

    protected virtual string GetJsonText()
    {
        var dict = new SortedDictionary<string, object>();
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
        return text;
    }

    public ParameterDefinitionValuePair[] GetDefinitionValueCollection()
    {
        var list = new List<ParameterDefinitionValuePair>();
        foreach (var definition in DefinitionCollection.Values)
        {
            list.Add(new(definition, Get(definition.Key)));
        }

        return list.ToArray();
    }

    public Dictionary<string, object?> GetValueDictionary()
    {
        var dict = new Dictionary<string, object?>();
        foreach (var definition in DefinitionCollection.Values)
        {
            dict.Add(definition.Key, Get(definition.Key));
        }

        if (AllowAbsentDefinition)
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
