using Poltergeist.Automations.Processors;
using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.Macros;

public class MacroProcessorLaunchedEvent(IMacroProcessor processor) : AppEvent
{
    public IMacroProcessor Processor => processor;
}
