namespace Poltergeist.Automations.Macros;

public class MacroStorage : Dictionary<string, object>
{
    public T? Get<T>(string key)
    {
        return (T?)this[key];
    }

    public T? Get<T>()
    {
        return (T?)Values.FirstOrDefault(x => x is T);
    }

    public T? Get<T>(Func<T, bool> selector)
    {
        return (T?)Values.FirstOrDefault(x => x is T t && selector(t));
    }

    public T[] GetAll<T>()
    {
        return Values.OfType<T>().ToArray();
    }

}
