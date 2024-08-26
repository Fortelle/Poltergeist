using Poltergeist.Android;
using Poltergeist.Automations.Utilities.Maths;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.Macros;

public interface IOperator
{
    public CapturingProvider Capturing { get; }
    public IEmulatorInputProvider Hand { get; }
    public TimerService Timer { get; }
    public RandomEx Random { get; }
}
