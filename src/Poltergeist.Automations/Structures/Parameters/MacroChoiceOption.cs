using System.Diagnostics.CodeAnalysis;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Automations.Structures.Parameters;

public class MacroChoiceOption : OptionDefinition<string>, IMacroChoiceOption
{
    public required Func<IMacroInformation, bool> Predicate { get; init; }

    public MacroChoiceOption(string key) : base(key, string.Empty)
    {
    }

    [SetsRequiredMembers]
    public MacroChoiceOption(string key, Func<IMacroInformation, bool> predicate) : base(key, string.Empty)
    {
        Predicate = predicate;
    }

    bool IMacroChoiceOption.Predicate(IMacroInformation info) => Predicate(info);
}
