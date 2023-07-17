using System;
using System.Collections.Generic;
using System.Linq;

namespace Poltergeist.Automations.Macros;

public class MacroStorage
{
    private List<object> Items { get; set; } = new();

    public void Add(object item)
    {
        Items.Add(item);
    }

    public T? Get<T>()
    {
        return (T?)Items.FirstOrDefault(x => x is T);
    }

    public T? Get<T>(Func<T, bool> selector)
    {
        return (T?)Items.FirstOrDefault(x => x is T t && selector(t));
    }

    public T[] GetAll<T>()
    {
        return Items.OfType<T>().ToArray();
    }

}
