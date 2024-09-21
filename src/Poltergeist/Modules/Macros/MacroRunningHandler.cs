using Poltergeist.Automations.Processors;
using Poltergeist.Modules.Events;

namespace Poltergeist.Modules.Macros;

public class MacroRunningHandler(IFrontProcessor processor) : AppEventHandler
{
    public IFrontProcessor Processor => processor;
}
