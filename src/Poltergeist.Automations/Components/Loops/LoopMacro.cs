using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Loops;

public class LoopMacro : CommonMacroBase
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

    public LoopMacro(string? name = null) : base(name)
    {
        Modules.Add(new LoopModule(LoopOptions));
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
            loopService.BeforeProc = () =>
            {
                var args = processor.GetService<LoopBeforeArguments>();
                Before(args);
                return !args.Cancel;
            };
        }

        if (Iterate is not null)
        {
            loopService.IterationProc = (index) =>
            {
                var args = processor.GetService<IterationArguments>();
                args.Index = index;
                Iterate.Invoke(args);
                return !args.Break;
            };
        }

        if (CheckContinue is not null)
        {
            loopService.IntermissionProc = (index) =>
            {
                var args = processor.GetService<LoopCheckContinueArguments>();
                args.IterationIndex = index;
                CheckContinue(args);
                return !args.Break;
            };
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
