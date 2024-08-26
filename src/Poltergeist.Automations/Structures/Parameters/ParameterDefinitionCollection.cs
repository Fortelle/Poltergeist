using System.Collections.ObjectModel;

namespace Poltergeist.Automations.Structures.Parameters;

public class ParameterDefinitionCollection : KeyedCollection<string, IParameterDefinition>
{
    protected override string GetKeyForItem(IParameterDefinition item) => item.Key;

    public void Add<T>(string key, T? value)
    {
        Add(new ParameterDefinition<T>(key, value));
    }
}
