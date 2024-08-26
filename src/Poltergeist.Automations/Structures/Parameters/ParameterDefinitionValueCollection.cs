using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Poltergeist.Automations.Utilities.Cryptology;

namespace Poltergeist.Automations.Structures.Parameters;

public class ParameterDefinitionValueCollection
{
    private ParameterDefinitionCollection Definitions { get; }
    private Dictionary<string, object> Values { get; } = new();

    private string? Filepath { get; set; }
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
        if (value is null)
        {
            Values.Remove(key);
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

    public object? Get(string key)
    {
        if (Values.TryGetValue(key, out var value))
        {
            return value;
        }

        if (Definitions.TryGetValue(key, out var definition))
        {
            return definition.DefaultValue;
        }

        return default;
    }

    public T? Get<T>(string key)
    {
        if (Values.TryGetValue(key, out var value))
        {
            return (T?)value;
        }

        if (Definitions.TryGetValue(key, out var definition))
        {
            return (T?)definition.DefaultValue;
        }

        return default;
    }

    public T? Get<T>(ParameterDefinition<T> definition)
    {
        if (Values.TryGetValue(definition.Key, out var value))
        {
            return (T?)value;
        }

        return (T?)definition.DefaultValue;
    }

    public T? Get<T>(IParameterDefinition definition)
    {
        if (Values.TryGetValue(definition.Key, out var value))
        {
            return (T?)value;
        }

        return (T?)definition.DefaultValue;
    }

    public void Load(string path)
    {
        Filepath = path;

        if (!File.Exists(Filepath))
        {
            return;
        }

        using var sr = new StreamReader(Filepath);
        using var reader = new JsonTextReader(sr);
        var dict = JObject.Load(reader);

        foreach (var (key, jtoken) in dict)
        {
            if (jtoken is null)
            {
                continue;
            }

            if (!Definitions.TryGetValue(key, out var definition))
            {
                Values[key] = jtoken;
                continue;
            }

            if (definition.Status == ParameterStatus.Deprecated)
            {
                continue;
            }

            try
            {
                var value = jtoken.ToObject(definition.BaseType)!;
                Values[key] = value;
            }
            catch (Exception)
            {
            }
        }
    }

    public void Save()
    {
        if (!HasChanged)
        {
            return;
        };

        if (string.IsNullOrEmpty(Filepath))
        {
            return;
        }

        if (Values.Count == 0)
        {
            return;
        }

        SaveAs(Filepath);
        HasChanged = false;
    }

    public void SaveAs(string path)
    {
        var dict = new Dictionary<string, object>();
        foreach (var (key, value) in Values)
        {
            if (value is null)
            {
                continue;
            }
            if (Definitions.TryGetValue(key, out var definition))
            {
                if (value == definition.DefaultValue)
                {
                    continue;
                }
            }
            dict.Add(key, value);
        }

        SerializationUtil.JsonSave(path, dict);
    }

    public ParameterDefinitionValuePair[] ToDefinitionValueArray()
    {
        var list = new List<ParameterDefinitionValuePair>();
        foreach (var definition in Definitions)
        {
            if (Values.TryGetValue(definition.Key, out var value))
            {
                list.Add(new(definition, value));
            }
            else
            {
                list.Add(new(definition, definition.DefaultValue));
            }
        }

        return list.ToArray();
    }
}
