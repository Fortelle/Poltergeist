using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Utilities.Maths;
using Poltergeist.Operations.Capturing;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Android.HybridEmulators;

public class HybridOperator : MacroService, IHybridOperator
{
    public CapturingProvider Capturer { get; }
    public IHybridInputService Hand { get; }
    public TimerService Timer { get; }
    public RandomEx Random { get; }

    public HybridOperator(MacroProcessor processor,
        CapturingProvider capturer,
        IHybridInputService hand,
        TimerService timer,
        RandomEx random
        ) : base(processor)
    {
        Capturer = capturer;
        Hand = hand;
        Timer = timer;
        Random = random;
    }
}
