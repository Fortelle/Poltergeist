using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Poltergeist.Common.Utilities.Cryptology;

namespace Poltergeist.Automations.Configs;

public class MacroOptions : IEnumerable<IOptionItem>
{
    private List<IOptionItem> Items { get; } = new();
    public bool HasChanged => Items.Count > 0 && Items.Any(x => x.HasChanged);

    public IEnumerator<IOptionItem> GetEnumerator() => Items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

    public Dictionary<string, object?> ToDictionary() => Items.ToDictionary(x => x.Key, x => x.Value);

    public MacroOptions()
    {
    }
    
    public void Add(IOptionItem item)
    {
        var i = Items.FindIndex(x => x.Key == item.Key);
        if (i > -1)
        {
            if(Items[i] is UndefinedOptionItem uoi && item.IsDefault)
            {
                item.Value = uoi.Value?.ToObject(item.BaseType);
            }
            Items[i] = item;
        }
        else
        {
            Items.Add(item);
        }
    }

    public void Add<T>(string key, T value)
    {
        Add(new OptionItem<T>(key, value));
    }

    public T? Get<T>(string key, T? defaultValue = default)
    {
        var item = Items.FirstOrDefault(x => x.Key == key);

        if (item is null)
        {
            return defaultValue;
        }
        else if (item is UndefinedOptionItem uoi)
        {
            if (uoi.Value is null)
            {
                return defaultValue;
            }

            try
            {
                return uoi.Value.ToObject<T>();
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
        else if (item.Value is T t)
        {
            return t;
        }
        else if (item.Default is T d)
        {
            return d;
        }
        else
        {
            return defaultValue;
        }
    }

    public void Set<T>(string key, T value)
    {
        var item = Items.FirstOrDefault(x => x.Key == key);
        if (item != null)
        {
            item.Value = value;
        }
    }

    // todo: error handler
    public void Load(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        using var sr = new StreamReader(path);
        using var reader = new JsonTextReader(sr);
        var dict = JObject.Load(reader);

        var undefinedItems = new List<UndefinedOptionItem>();
        foreach (var (key, jtoken) in dict)
        {
            if (jtoken is null)
            {
                continue;
            }

            var existingItem = this.FirstOrDefault(x => x.Key == key);
            if(existingItem is not null)
            {
                try
                {
                    existingItem.Value = jtoken.ToObject(existingItem.BaseType);
                    existingItem.HasChanged = false;
                }
                catch (Exception)
                {
                }
            }
            else
            {
                undefinedItems.Add(new(key, jtoken));
            }
        }

        foreach (var item in undefinedItems)
        {
            Items.Add(item);
        }
    }

    public void Save(string path)
    {
        var dict = new Dictionary<string, object>();
        foreach (var item in Items)
        {
            if (!item.IsDefault && item.Value is not null)
            {
                dict.Add(item.Key, item.Value);
            }
            item.HasChanged = false;
        }
        SerializationUtil.JsonSave(path, dict);
    }

}
