using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Processors;

public class ProcessorEndingHook : MacroHook
{
    public required EndReason Reason { get; init; }
    public required DateTime StartTime { get; init; }
    public required TimeSpan Duration { get; init; }

    public string? Comment { get; set; }

    public CompletionAction CompletionAction { get; set; }
}
