using System.Collections;

namespace Poltergeist.Automations.Structures.Parameters;

public class ObservableParameterCollection : IEnumerable<ObservableParameterItem>
{
    private readonly List<ObservableParameterItem> Items = new();

    public ObservableParameterCollection()
    {
    }

    public ObservableParameterCollection(ParameterDefinitionValueCollection collection)
    {
        foreach (var (def, value) in collection.GetDefinitionValueCollection())
        {
            var item = new ObservableParameterItem(def, value);
            item.Changed += () =>
            {
                collection.Set(def.Key, item.Value);
            };
            Items.Add(item);
        }
    }

    public IEnumerator<ObservableParameterItem> GetEnumerator() => Items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
}
