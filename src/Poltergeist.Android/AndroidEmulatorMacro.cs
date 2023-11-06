using Poltergeist.Android.Emulators;
using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Components.Repetitions;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Operations.Macros;

public class AndroidEmulatorMacro : MacroBase
{
    private AndroidEmulatorOperator? Operator { get; set; }

    public Action<LoopBeforeArguments, AndroidEmulatorOperator>? Before;
    public Action<LoopExecutionArguments, AndroidEmulatorOperator>? Execution;
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
    }

    protected override void OnInitialize()
    {
        Modules.Add(new LoopModule(LoopOptions));
        Modules.Add(new EmulatorModule());
        Modules.Add(new InputOptionsModule());
    }

    protected override void OnProcess(MacroProcessor processor)
    {
        base.OnProcess(processor);

        var repeat = processor.GetService<LoopService>();
        repeat.Before += OnBegin;
        if (Execution != null)
        {
            repeat.Execution = (e) => Execution.Invoke(e, Operator!);
        }

        if (CheckContinue != null)
        {
            repeat.CheckContinue = (e) => CheckContinue.Invoke(e, Operator!);
        }

        if (After != null)
        {
            repeat.After = (e) => After.Invoke(e, Operator!);
        }
    }

    private void OnBegin(LoopBeforeArguments e)
    {
        Operator = e.Processor.GetService<AndroidEmulatorOperator>();

        Before?.Invoke(e, Operator);
    }
}
