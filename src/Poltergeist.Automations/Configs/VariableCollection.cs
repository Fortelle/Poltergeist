using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Poltergeist.Common.Utilities.Cryptology;

namespace Poltergeist.Automations.Configs;

public class VariableCollection : IEnumerable<VariableItem>
{
    private List<VariableItem> Items { get; } = new();

    public IEnumerator<VariableItem> GetEnumerator() => Items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

    public VariableCollection()
    {
    }

    public void Add(VariableItem item)
    {
        var i = Items.FindIndex(x => x.Key == item.Key);
        if (i > -1)
        {
            throw new ArgumentException("An element with the same key already exists.");
        }

        Items.Add(item);
    }

    public void Add(string key, object value)
    {
        Add(new(key, value));
    }

    public T Get<T>(string key, T defaultValue)
    {
        var item = Items.FirstOrDefault(x => x.Key == key);
        if (item is null)
        {
            throw new KeyNotFoundException($"The key \"{key}\" does not exist in the {nameof(VariableCollection)}.");
        }

        if(item.Value is null)
        {
            return defaultValue;
        }

        return (T)item.Value;
    }

    public void Set(string key, object value)
    {
        var item = Items.FirstOrDefault(x => x.Key == key);
        if (item is not null)
        {
            item.Value = value;
        }
        else
        {
            Add(key, value);
        }
    }

    public void Set<T>(string key, Func<T, T> action) where T : notnull
    {
        var item = Items.FirstOrDefault(x => x.Key == key);
        if (item is null)
        {
            throw new KeyNotFoundException($"The key \"{key}\" does not exist in the {nameof(VariableCollection)}.");
        }
        if (item.Value is not T value)
        {
            throw new ArgumentException($"The value of \"{key}\" is not {typeof(T)}.");
        }

        item.Value = action(value);
    }

    public void Load(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        using var sr = new StreamReader(path);
        using var reader = new JsonTextReader(sr);
        var dict = JObject.Load(reader);

        foreach (var (key, jtoken) in dict)
        {
            if (jtoken is null)
            {
                continue;
            }

            var existingItem = this.FirstOrDefault(x => x.Key == key);
            if (existingItem is null)
            {
                continue;
            }

            try
            {
                existingItem.Value = jtoken.ToObject(existingItem.BaseType)!;
            }
            catch (Exception)
            {
            }
        }
    }

    public void Save(string path)
    {
        var dict = Items.ToDictionary(x => x.Key, x => x.Value);
        SerializationUtil.JsonSave(path, dict);
    }

}
