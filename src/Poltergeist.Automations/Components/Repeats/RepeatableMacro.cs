using System;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Components.Loops;

namespace Poltergeist.Automations.Components.Repeats;

public class RepeatableMacro : MacroBase
{
    public Func<MacroProcessor, bool> Begin;
    public Func<MacroProcessor, IterationResult> Iteration;
    public Func<MacroProcessor, CheckNextResult> CheckNext;
    public Action<MacroProcessor> End;

    public RepeatableMacro(string name) : base(name)
    {
        Modules.Add(new RepeatModule()
        {
            Options = {
                UseCount = true,
                UseTimeout = true,
                Instrument =  RepeatInstrumentType.List,
            }
        });
    }

    protected internal override void OnConfigure(MacroServiceCollection services)
    {
        base.OnConfigure(services);
    }

    protected internal override void OnProcess(MacroProcessor processor)
    {
        base.OnProcess(processor);

        var loop = processor.GetService<RepeatService>();
        if (Begin != null) loop.BeginProc = () => Begin.Invoke(processor);
        if (Iteration != null) loop.IterationProc = () => Iteration.Invoke(processor);
        if (CheckNext != null) loop.CheckNextProc = () => CheckNext.Invoke(processor);
        if (End != null) loop.EndProc = () => End.Invoke(processor);
    }

}
