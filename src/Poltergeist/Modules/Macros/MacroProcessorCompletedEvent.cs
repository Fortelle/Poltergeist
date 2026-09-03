using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.Macros;

public class MacroProcessorCompletedEvent : AppEvent
{
    public required ProcessorResult Result { get; init; }

    public MacroInstance? Instance { get; init; }

    public IMacroBase? Macro => Instance?.Template;
}
