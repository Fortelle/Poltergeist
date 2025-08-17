using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Poltergeist.Automations.Structures.Parameters;

public class PredefinedCollection
{
    public bool AllowAbsentDefinition { get; init; }

    public int DefinitionCount => DefinitionCollection.Count;

    public bool HasChanged { get; private set; }

    protected readonly Dictionary<string, IParameterDefinition> DefinitionCollection = new();
    protected readonly Dictionary<string, object?> ValueCollection = new();

    protected readonly object _lock = new();

    public PredefinedCollection()
    {
    }

    public PredefinedCollection(KeyedCollection<string, IParameterDefinition> definitions)
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
            DefinitionCollection.Remove(key);
            ValueCollection.Remove(key);
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

        value ??= definition?.DefaultValue;

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
        }

        return dict;
    }
}
