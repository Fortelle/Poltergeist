﻿using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Loops;

public class LoopMacro : MacroBase
{
    public LoopOptions LoopOptions { get; } = new()
    {
        IsCountLimitable = true,
        IsDurationLimitable = true,
        Instrument = LoopInstrumentType.List,
    };

    public Action<LoopBeforeArguments>? Before;
    public Action<IterationArguments>? Iterate;
    public Action<LoopCheckContinueArguments>? CheckContinue;
    public Action<ArgumentService>? After;

    public LoopMacro(string name) : base(name)
    {
        Modules.Add(new LoopModule(LoopOptions));
        Modules.Add(new CompleteModule());
    }

    protected override void OnConfigure(IConfigurableProcessor processor)
    {
        base.OnConfigure(processor);
    }

    protected override void OnPrepare(IPreparableProcessor processor)
    {
        base.OnPrepare(processor);

        var loopService = processor.GetService<LoopService>();
        var hookService = processor.GetService<HookService>();

        if (Before is not null)
        {
            hookService.Register<LoopStartingHook>(hook =>
            {
                var args = hook.Processor.GetService<LoopBeforeArguments>();
                Before(args);
                if (args.Cancel)
                {
                    hook.Cancel = true;
                }
            });
        }

        if (Iterate is not null)
        {
            hookService.Register<IterationExecutingHook>(hook =>
            {
                var args = hook.Processor.GetService<IterationArguments>();
                args.Index = hook.Index;
                args.Result = IterationResult.Continue;
                Iterate(args);
                hook.Result = args.Result;
            });
        }

        if (CheckContinue is not null)
        {
            hookService.Register<LoopCheckContinueHook>(hook =>
            {
                var args = hook.Processor.GetService<LoopCheckContinueArguments>();
                args.IterationIndex = hook.Data.Index;
                args.IterationData = hook.Data;
                CheckContinue(args);
                hook.Result = args.Result;
            });
        }

        if (After is not null)
        {
            hookService.Register<LoopEndedHook>(hook =>
            {
                var args = hook.Processor.GetService<ArgumentService>();
                After(args);
            });
        }
    }

}
