using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Processors;

public class ProcessorEndedHook : MacroHook
{
    public required ProcessHistoryEntry HistoryEntry { get; init; }
    public required CompletionAction CompletionAction { get; set; }
}
