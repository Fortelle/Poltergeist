using System.Collections.ObjectModel;

namespace Poltergeist.Automations.Structures.Parameters;

public class OptionDefinitionCollection : KeyedCollection<string, IOptionDefinition>
{
    protected override string GetKeyForItem(IOptionDefinition item) => item.Key;
}
