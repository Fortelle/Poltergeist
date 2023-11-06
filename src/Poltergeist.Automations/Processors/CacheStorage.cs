using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Poltergeist.Automations.Processors;

public class CacheStorage : IEnumerable<KeyValuePair<string, object>>
{
    private Dictionary<string, object> Items { get; set; } = new();

    public void Add(string key, object item)
    {
        if(Items.ContainsKey(key))
        {
            Items[key] = item;
        }
        else
        {
            Items.Add(key, item);
        }
    }

    public T? Get<T>(string key)
    {
        if (Items.TryGetValue(key, out var cache) && cache is T t)
        {
            return t;
        }
        else
        {
            return default;
        }
    }

    public T? Get<T>()
    {
        return (T?)Items.Values.FirstOrDefault(x => x is T);
    }

    public T? Get<T>(Func<T, bool> selector)
    {
        return (T?)Items.Values.FirstOrDefault(x => x is T t && selector(t));
    }

    public T[] GetAll<T>()
    {
        return Items.Values.OfType<T>().ToArray();
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => Items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
}
