using Poltergeist.Android.Emulators;
using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Operations.Macros;

public class AndroidEmulatorMacro : MacroBase
{
    public Action<LoopBeforeArguments, AndroidEmulatorOperator>? Before;
    public Action<LoopExecuteArguments, AndroidEmulatorOperator>? Execute;
    public Action<LoopCheckContinueArguments, AndroidEmulatorOperator>? CheckContinue;
    public Action<ArgumentService, AndroidEmulatorOperator>? After;

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

        var repeat = processor.GetService<LoopService>();

        if (Before != null)
        {
            repeat.Before = (e) => Before.Invoke(e, e.Processor.GetService<AndroidEmulatorOperator>());
        }

        if (Execute != null)
        {
            repeat.Execute = (e) => Execute.Invoke(e, e.Processor.GetService<AndroidEmulatorOperator>());
        }

        if (CheckContinue != null)
        {
            repeat.CheckContinue = (e) => CheckContinue.Invoke(e, e.Processor.GetService<AndroidEmulatorOperator>());
        }

        if (After != null)
        {
            repeat.After = (e) => After.Invoke(e, e.Processor.GetService<AndroidEmulatorOperator>());
        }
    }

}
