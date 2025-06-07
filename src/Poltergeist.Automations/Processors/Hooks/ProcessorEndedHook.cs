using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Processors;

public class ProcessorEndedHook : MacroHook
{
    public required ProcessHistoryEntry HistoryEntry { get; init; }
    public required CompletionAction CompletionAction { get; set; }
    public required ParameterValueCollection OutputStorage { get; init; }
}
