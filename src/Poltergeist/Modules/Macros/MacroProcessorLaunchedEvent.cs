using Poltergeist.Automations.Processors;
using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.Macros;

public class MacroProcessorLaunchedEvent(IFrontProcessor processor) : AppEvent
{
    public IFrontProcessor Processor => processor;
}
