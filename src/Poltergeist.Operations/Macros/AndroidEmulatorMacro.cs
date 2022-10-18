using System;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Components.Loops;
using Poltergeist.Operations.AndroidEmulators;

namespace Poltergeist.Operations.Macros;

public class AndroidEmulatorMacro : MacroBase
{
    private AndroidEmulatorOperator Operator { get; set; }

    public RegionConfig RegionConfig { get; set; }
     
    public Action<LoopBeginArguments, AndroidEmulatorOperator> Begin;
    public Action<LoopIterationArguments, AndroidEmulatorOperator> Iteration;
    public Action<LoopCheckNextArguments, AndroidEmulatorOperator> CheckNext;
    public Action<ArgumentService, AndroidEmulatorOperator> End;

    public AndroidEmulatorMacro(string name) : base(name)
    {
        Modules.Add(new RepeatModule());
        Modules.Add(new EmulatorModule());
    }

    protected override void OnProcess(MacroProcessor processor)
    {
        base.OnProcess(processor);

        var repeat = processor.GetService<RepeatService>();
        repeat.BeginProc += OnBegin;
        if (Iteration != null) repeat.IterationProc = (e) => Iteration.Invoke(e, Operator);
        if (CheckNext != null) repeat.CheckNextProc = (e) => CheckNext.Invoke(e, Operator);
        if (End != null) repeat.EndProc = (e) => End.Invoke(e, Operator);
    }

    private void OnBegin(LoopBeginArguments e)
    {
        Operator = e.Processor.GetService<AndroidEmulatorOperator>();

        Begin?.Invoke(e, Operator);
    }
}
