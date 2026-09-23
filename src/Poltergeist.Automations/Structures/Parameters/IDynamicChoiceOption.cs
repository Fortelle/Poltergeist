using Poltergeist.Automations.Macros;

namespace Poltergeist.Automations.Structures.Parameters;

public interface IDynamicChoiceOption
{
    ChoiceEntry[] GetChoices();
}
