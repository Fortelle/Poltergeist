using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;

namespace Poltergeist.Automations.Structures.Parameters;

public class ParameterValueCollection : KeyedCollection<string, ParameterValue>
{
    protected override string GetKeyForItem(ParameterValue value)
    {
        return value.Key;
    }

    public void Add(string key, object? value)
    {
        Add(new ParameterValue()
        {
            Key = key,
            Value = value,
            HasChanged = false
        });
    }

    public void Add(IParameterDefinition definition, object? value)
    {
        Add(definition.Key, value);
    }

    public void Reset(string key, object? value)
    {
        if (TryGetValue(key, out var entry))
        {
            entry.Value = value;
            entry.HasChanged = false;
        }
        else
        {
            Add(new ParameterValue()
            {
                Key = key,
                Value = value,
                HasChanged = false
            });
        }
    }

    public void Set(string key, object? value)
    {
        if (!TryGetValue(key, out var entry))
        {
            throw new KeyNotFoundException();
        }

        entry.Value = value;
        entry.HasChanged = true;
    }

    public void Set(IParameterDefinition definition, object? value)
    {
        Set(definition.Key, value);
    }

    public void Set<T>(string key, Func<T, T> action)
    {
        var oldValue = Get<T>(key)!;
        var newValue = action(oldValue)!;
        Set(key, newValue);
    }

    public void Set<T>(ParameterDefinition<T> definition, Func<T, T> action)
    {
        Set(definition.Key, action);
    }

    public object? Get(string key)
    {
        if (!TryGetValue(key, out var entry))
        {
            return default;
            //throw new KeyNotFoundException();
        }

        if (entry.Value is null)
        {
            return default;
        }

        return entry.Value;
    }

    public T? Get<T>(string key)
    {
        if (!TryGetValue(key, out var entry))
        {
            return default;
            //throw new KeyNotFoundException();
        }

        if (entry.Value is null)
        {
            return default;
        }

        if (entry.Value is not T value)
        {
            throw new InvalidCastException();
        }

        return value;
    }

    public T? Get<T>(string key, T defaultValue)
    {
        if (!TryGetValue(key, out var entry))
        {
            return defaultValue;
        }

        if (entry.Value is not T value)
        {
            throw new InvalidCastException();
        }

        return value;
    }

    public T? Get<T>(ParameterDefinition<T> definition)
    {
        return Get<T>(definition.Key);
    }
}
