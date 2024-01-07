using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Loops;

public class LoopExecuteArguments : ArgumentService
{
    public int Index { get; internal set; }
    public DateTime StartTime { get; internal set; }
    public IterationStatus Result { get; set; }

    public int? ProgressMax { get; set; }
    internal event Action<ProgressInstrumentInfo>? Reported;

    public LoopExecuteArguments(MacroProcessor processor) : base(processor)
    {
    }

    public void Report(ProgressInstrumentInfo info)
    {
        Reported?.Invoke(info);
    }
}
