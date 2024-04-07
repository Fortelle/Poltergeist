using System.Collections;

namespace Poltergeist.Automations.Parameters;

public class ObservableParameterCollection : IEnumerable<ObservableParameterItem>
{
    public List<ObservableParameterItem> Items { get; } = new();

    public ObservableParameterCollection()
    {
    }

    public ObservableParameterCollection(ParameterDefinitionValueCollection collection)
    {
        foreach (var (def, value) in collection.ToDefinitionValueArray())
        {
            var item = new ObservableParameterItem(def, value);
            item.Changed += () =>
            {
                collection.Set(def, item.Value);
            };
            Items.Add(item);
        }
    }

    public IEnumerator<ObservableParameterItem> GetEnumerator() => Items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
}
