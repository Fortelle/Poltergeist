using System.Collections;

namespace Poltergeist.Automations.Configs;

public interface IChoiceOptionItem
{
    public IEnumerable Choices { get; }
}
