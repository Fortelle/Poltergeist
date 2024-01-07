using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Processors;

public class ProcessorEndingHook : MacroHook
{
    public EndReason Reason { get; init; }
    public ProcessHistoryEntry HistoryEntry { get; init; }
    public CompletionAction CompletionAction { get; set; }
}
