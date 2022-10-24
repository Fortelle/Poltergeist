using System;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Components.Loops;

namespace Poltergeist.Automations.Components.Repeats;

public class RepeatableMacro<T> : MacroBase
{
    public T Argument;

    public Action<LoopBeginArguments, T> Begin;
    public Action<LoopIterationArguments, T> Iteration;
    public Action<LoopCheckNextArguments, T> CheckNext;
    public Action<ArgumentService, T> End;

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
        Modules.Add(new CompleteModule());
    }

    protected internal override void OnConfigure(MacroServiceCollection services, IConfigureProcessor processor)
    {
        base.OnConfigure(services, processor);
    }

    protected internal override void OnProcess(MacroProcessor processor)
    {
        base.OnProcess(processor);

        var loop = processor.GetService<RepeatService>();
        if (Begin != null) loop.BeginProc += e => Begin(e, Argument);
        if (Iteration != null) loop.IterationProc += e => Iteration(e, Argument);
        if (CheckNext != null) loop.CheckNextProc += e => CheckNext(e, Argument);
        if (End != null) loop.EndProc += e => End(e, Argument);
    }

}
