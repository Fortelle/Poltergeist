using System.Collections;

namespace Poltergeist.Automations.Configs;

public interface IChoiceOptionItem : IOptionItem
{
    public IEnumerable Choices { get; }
    public ChoiceOptionMode Mode { get; }
}
