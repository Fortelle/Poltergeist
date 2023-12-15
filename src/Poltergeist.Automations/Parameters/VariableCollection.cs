using System.Collections.ObjectModel;

namespace Poltergeist.Automations.Parameters;

public class VariableCollection : KeyedCollection<string, VariableEntry>
{
    protected override string GetKeyForItem(VariableEntry item)
    {
        return item.Key;
    }

    public void Add<T>(string key, T value, ParameterSource source, bool isChanged = false)
    {
        this.Add(new VariableEntry()
        {
            Key = key,
            Value = value,
            Source = source,
            HasChanged = isChanged,
        });
    }

    public void AddRange(IReadOnlyDictionary<string, object?> variables, ParameterSource source, bool isChanged = false)
    {
        foreach (var (key, value) in variables)
        {
            Add(key, value, source, isChanged);
        }
    }

    public void AddRange(IEnumerable<VariableEntry> variables)
    {
        foreach (var item in variables)
        {
            this.Add(item);
        }
    }

    public object? Get(string key)
    {
        return this[key].Value;
    }

    public T Get<T>(string key)
    {
        return (T)this[key].Value;
    }

    public T Get<T>(ParameterEntry<T> param)
    {
        return (T)this[param.Key].Value;
    }

    public T Get<T>(string key, T defaultValue)
    {
        if (!this.TryGetValue(key, out var entry))
        {
            return defaultValue;
        }

        return (T)entry.Value;
    }

    public void Set<T>(string key, T value, ParameterSource? source = null)
    {
        if (this.Contains(key))
        {
            this[key].Value = value;
            this[key].HasChanged = true;
        }
        else
        {
            Add(key, value, source ?? ParameterSource.Macro, true);
        }
    }

    public void Set<T>(string key, Func<T, T> action)
    {
        var value = Get<T>(key);
        var newValue = action(value);
        Set(key, newValue);
    }

    public void Set<T>(ParameterEntry<T> param, T value)
    {
        Set(param.Key, value);
    }

    public void Set<T>(ParameterEntry<T> param, Func<T, T> action)
    {
        Set(param.Key, action);
    }

    public IReadOnlyDictionary<string, object?> ToValueDictionary()
    {
        return this.ToDictionary(x => x.Key, x => x.Value);
    }

    public IReadOnlyDictionary<string, object?> ToValueDictionary(ParameterSource source)
    {
        return this
            .Where(x => x.HasChanged && x.Source == source)
            .ToDictionary(x => x.Key, x => x.Value);
    }
}
