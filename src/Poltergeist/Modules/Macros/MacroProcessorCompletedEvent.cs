using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.Macros;

public class MacroProcessorCompletedEvent : AppEvent
{
    public required EndReason Reason { get; init; }

    public required ProcessorResult Result { get; init; }

    public required IFrontMacro Macro { get; init; }

    public MacroInstance? Instance { get; init; }
}
