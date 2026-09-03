using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Macros;

public class ActionContext
{
    public required IMacroInformation Macro { get; init; }

    public required IReadOnlyParameterValueCollection Options { get; init; }

    public required IReadOnlyParameterValueCollection Environments { get; init; }
}
