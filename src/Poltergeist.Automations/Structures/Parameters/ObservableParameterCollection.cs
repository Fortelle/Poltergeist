using System.Collections;

namespace Poltergeist.Automations.Structures.Parameters;

public class ObservableParameterCollection : IEnumerable<ObservableParameterItem>
{
    public delegate void ChangedEventHandler(string key, object? oldValue, object? newValue);

    private readonly List<ObservableParameterItem> Items = new();

    public event ChangedEventHandler? Changed;

    public ObservableParameterCollection()
    {
    }

    public ObservableParameterCollection(SavablePredefinedCollection collection)
    {
        foreach (var (def, value) in collection.GetDefinitionValueCollection())
        {
            var item = new ObservableParameterItem(def, value);
            item.Changed += (key, oldValue, newValue) =>
            {
                collection.Set(key, newValue);
                Changed?.Invoke(key, oldValue, newValue);
            };
            Items.Add(item);
        }
    }

    public void Add(ObservableParameterItem item)
    {
        Items.Add(item);
    }

    public IEnumerator<ObservableParameterItem> GetEnumerator() => Items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
}
