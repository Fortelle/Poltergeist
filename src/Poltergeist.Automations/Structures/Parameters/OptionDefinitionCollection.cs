using System.Collections.ObjectModel;

namespace Poltergeist.Automations.Structures.Parameters;

public class OptionDefinitionCollection : KeyedCollection<string, IOptionDefinition>
{
    protected override string GetKeyForItem(IOptionDefinition item) => item.Key;

    public void Add<T>(string key, T value) where T : notnull
    {
        Add(new OptionDefinition<T>(key, value));
    }
}
