﻿using System;
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
                UsePersistence = true,
                Instrument =  RepeatInstrumentType.List,
            }
        });
    }

    protected internal override void ConfigureProc(MacroServiceCollection services)
    {
        base.ConfigureProc(services);
    }

    protected internal override void ReadyProc(MacroProcessor processor)
    {
        base.ReadyProc(processor);

        var loop = processor.GetService<RepeatService>();
        if (Begin != null) loop.BeginProc = () => Begin.Invoke(processor);
        if (Iteration != null) loop.IterationProc = () => Iteration.Invoke(processor);
        if (CheckNext != null) loop.CheckNextProc = () => CheckNext.Invoke(processor);
        if (End != null) loop.EndProc = () => End.Invoke(processor);
    }

}
