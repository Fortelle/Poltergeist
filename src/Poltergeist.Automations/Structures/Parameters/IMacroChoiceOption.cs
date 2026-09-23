using Poltergeist.Automations.Macros;

namespace Poltergeist.Automations.Structures.Parameters;

public interface IMacroChoiceOption
{
    bool Predicate(IMacroInformation info);
}
