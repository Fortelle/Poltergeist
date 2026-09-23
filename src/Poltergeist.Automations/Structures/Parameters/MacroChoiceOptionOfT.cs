using Poltergeist.Automations.Macros;

namespace Poltergeist.Automations.Structures.Parameters;

public class MacroChoiceOption<T> : OptionDefinition<T>, IMacroChoiceOption where T : MacroBase
{
    public MacroChoiceOption(string key) : base(key)
    {
    }

    bool IMacroChoiceOption.Predicate(IMacroInformation info) => info is T;
}
