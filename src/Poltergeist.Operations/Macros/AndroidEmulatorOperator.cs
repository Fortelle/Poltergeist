using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Common.Utilities.Maths;
using Poltergeist.Operations.AndroidEmulators;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.Macros;

public class AndroidEmulatorOperator : MacroService
{
    public CapturingSource Capturing { get; }
    public IEmulatorInputSource Hand { get; }
    public TimerService Timer { get; }
    public RandomEx Random { get; }

    public AndroidEmulatorOperator(MacroProcessor processor,
        CapturingSource capturing,
        IEmulatorInputSource hand,
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
