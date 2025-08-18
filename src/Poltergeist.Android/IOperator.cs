using Poltergeist.Automations.Utilities.Maths;
using Poltergeist.Operations;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Android;

public interface IOperator
{
    CapturingProvider Capturing { get; }
    IEmulatorInputProvider Hand { get; }
    TimerService Timer { get; }
    RandomEx Random { get; }
}
