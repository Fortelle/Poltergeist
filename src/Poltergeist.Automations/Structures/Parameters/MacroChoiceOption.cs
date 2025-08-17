using System.Diagnostics.CodeAnalysis;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Automations.Structures.Parameters;

public class MacroChoiceOption : OptionDefinition<string>
{
    public required Func<MacroBase, bool> Predicate { get; init; }

    public MacroChoiceOption(string key) : base(key, string.Empty)
    {
    }

    [SetsRequiredMembers]
    public MacroChoiceOption(string key, Func<MacroBase, bool> predicate) : base(key, string.Empty)
    {
        Predicate = predicate;
    }
}
