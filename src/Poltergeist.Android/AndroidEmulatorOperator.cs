using Poltergeist.Android;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Common.Utilities.Maths;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.Macros;

public class AndroidEmulatorOperator : MacroService, IOperator
{
    public CapturingProvider Capturing { get; }
    public IEmulatorInputProvider Hand { get; }
    public TimerService Timer { get; }
    public RandomEx Random { get; }

    public AndroidEmulatorOperator(MacroProcessor processor,
        CapturingProvider capturing,
        IEmulatorInputProvider hand,
        TimerService timer,
        RandomEx random
        ) : base(processor)
    {
        Capturing = capturing;

        Hand = hand;
        Timer = timer;

        Random = random;
    }
}
