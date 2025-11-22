using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Operations.Hybrid;

public class HybridOperationService(MacroProcessor processor) : MacroService(processor)
{
    public void Connect()
    {
        Processor.GetService<HookService>().Raise<HybridOperationStartHook>();
    }

    public void Disconnect()
    {
        Processor.GetService<HookService>().Raise<HybridOperationStopHook>();
    }
}
