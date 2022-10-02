using System;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Components.Loops;

namespace Poltergeist.Automations.Components.Repeats;

public class RepeatableMacro : MacroBase
{
    public Action<LoopBeginArguments> Begin;
    public Action<LoopIterationArguments> Iteration;
    public Action<LoopCheckNextArguments> CheckNext;
    public Action<ArgumentService> End;

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
        if (Begin != null) loop.BeginProc = Begin;
        if (Iteration != null) loop.IterationProc = Iteration;
        if (CheckNext != null) loop.CheckNextProc = CheckNext;
        if (End != null) loop.EndProc = End;
    }

}
