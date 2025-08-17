using Poltergeist.Android.Emulators;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Operations.Macros;

public class AndroidEmulatorMacro : CommonMacroBase
{
    public Action<ArgumentService>? BeforeConnect;
    public Action<ArgumentService, AndroidEmulatorOperator>? AfterConnect;
    public Action<IterationArguments, AndroidEmulatorOperator>? Execute;
    public Action<LoopCheckContinueArguments, AndroidEmulatorOperator>? CheckContinue;
    public Action<ArgumentService, AndroidEmulatorOperator>? Finally;

    public LoopOptions LoopOptions { get; } = new()
    {
        IsCountLimitable = true,
        IsDurationLimitable = true,
        Instrument = LoopInstrumentType.List,
    };

    public AndroidEmulatorMacro(string name) : base(name)
    {
        Modules.Add(new LoopModule(LoopOptions));
        Modules.Add(new EmulatorModule());
        Modules.Add(new InputOptionsModule());
    }

    protected override void OnPrepare(IPreparableProcessor processor)
    {
        base.OnPrepare(processor);

        var loopService = processor.GetService<LoopService>();
        var hookService = processor.GetService<HookService>();

        hookService.Register<LoopStartedHook>(hook =>
        {
            BeforeConnect?.Invoke(processor.GetService<ArgumentService>());

            var emulatorService = processor.GetService<EmulatorService>();
            emulatorService.Connect();

            var ope = hook.Processor.GetService<AndroidEmulatorOperator>();
            AfterConnect?.Invoke(processor.GetService<ArgumentService>(), ope);
        });

        if (Execute is not null)
        {
            hookService.Register<IterationExecutingHook>(hook =>
            {
                var args = hook.Processor.GetService<IterationArguments>();
                args.Index = hook.Index;
                //args.StartTime = hook.StartTime;
                args.Result = IterationResult.Continue;

                var ope = hook.Processor.GetService<AndroidEmulatorOperator>();

                Execute(args, ope);
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
                var ope = hook.Processor.GetService<AndroidEmulatorOperator>();
                CheckContinue(args, ope);
                hook.Result = args.Result;
            });
        }

        if (Finally is not null)
        {
            hookService.Register<LoopEndingHook>(hook =>
            {
                var args = hook.Processor.GetService<ArgumentService>();
                var ope = hook.Processor.GetService<AndroidEmulatorOperator>();
                Finally(args, ope);
            });
        }

    }

}
