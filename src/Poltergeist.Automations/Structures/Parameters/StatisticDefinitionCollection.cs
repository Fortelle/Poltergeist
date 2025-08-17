using System.Collections.ObjectModel;

namespace Poltergeist.Automations.Structures.Parameters;

public class StatisticDefinitionCollection : KeyedCollection<string, IStatisticDefinition>
{
    protected override string GetKeyForItem(IStatisticDefinition item) => item.Key;
}
