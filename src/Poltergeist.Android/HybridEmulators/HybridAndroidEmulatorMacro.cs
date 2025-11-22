using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Operations.Hybrid;
using Poltergeist.Operations.Inputing;

namespace Poltergeist.Android.HybridEmulators;

public class HybridAndroidEmulatorMacro : CommonMacroBase
{
    public Action<ArgumentService>? BeforeConnect;
    public Action<ArgumentService, HybridOperator>? AfterConnect;
    public Action<IterationArguments, HybridOperator>? Execute;
    public Action<LoopCheckContinueArguments, HybridOperator>? CheckContinue;
    public Action<ArgumentService>? Finally;

    public LoopOptions LoopOptions { get; } = new()
    {
        IsCountLimitable = true,
        IsDurationLimitable = true,
        Instrument = LoopInstrumentType.List,
    };

    public HybridAndroidEmulatorMacro(string name) : base(name)
    {
        Modules.Add(new LoopModule(LoopOptions));
        Modules.Add(new HybridAndroidEmulatorModule());
        Modules.Add(new InputOptionsModule());
    }

    protected override void OnPrepare(IPreparableProcessor processor)
    {
        base.OnPrepare(processor);

        var loopService = processor.GetService<LoopService>();
        var hookService = processor.GetService<HookService>();

        loopService.BeforeProc = () =>
        {
            BeforeConnect?.Invoke(processor.GetService<ArgumentService>());

            var emulatorService = processor.GetService<HybridOperationService>();
            emulatorService.Connect();

            var ope = processor.GetService<HybridOperator>();
            AfterConnect?.Invoke(processor.GetService<ArgumentService>(), ope);

            return true;
        };

        if (Execute is not null)
        {
            loopService.IterationProc = (index) =>
            {
                var args = processor.GetService<IterationArguments>();
                args.Index = index;
                var ope = processor.GetService<HybridOperator>();
                Execute(args, ope);
                return !args.Break;
            };
        }

        if (CheckContinue is not null)
        {
            loopService.IntermissionProc = (index) =>
            {
                var args = processor.GetService<LoopCheckContinueArguments>();
                args.IterationIndex = index;
                var ope = processor.GetService<HybridOperator>();
                CheckContinue(args, ope);
                return !args.Break;
            };
        }

        if (Finally is not null)
        {
            hookService.Register<LoopEndingHook>(hook =>
            {
                var args = hook.Processor.GetService<ArgumentService>();
                Finally(args);
            });
        }
    }
}
