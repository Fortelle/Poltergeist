using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Loops;

public class IterationArguments(MacroProcessor processor) : ArgumentService(processor)
{
    public int Index { get; set; }
    public DateTime StartTime { get; set; }
    public IterationResult Result { get; set; }

    public void Report(ProgressInstrumentInfo info)
    {
        var hook = new UpdatetInstrumentInfoHook()
        {
            Index = Index,
            Info = info,
        };
        Processor.GetService<HookService>().Raise(hook);
    }
}
