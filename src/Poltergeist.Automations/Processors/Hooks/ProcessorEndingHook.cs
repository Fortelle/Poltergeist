using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Automations.Processors;

public class ProcessorEndingHook : MacroHook
{
    public required EndReason Reason { get; init; }
    public required DateTime StartTime { get; init; }
    public required TimeSpan Duration { get; init; }
    public required ParameterValueCollection OutputStorage { get; init; }
    
    public string? Comment { get; set; }

    public CompletionAction CompletionAction { get; set; }
}
