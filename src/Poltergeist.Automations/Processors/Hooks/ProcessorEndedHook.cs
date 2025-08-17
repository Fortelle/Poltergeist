using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Processors;

public class ProcessorEndedHook : MacroHook
{
    public required EndReason Reason { get; init; }
    public required ParameterValueCollection Report { get; init; }
}
