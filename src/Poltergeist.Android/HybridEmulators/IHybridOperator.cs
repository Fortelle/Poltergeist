using Poltergeist.Automations.Utilities.Maths;
using Poltergeist.Operations.Capturing;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Android.HybridEmulators;

public interface IHybridOperator
{
    CapturingProvider Capturer { get; }
    IHybridInputService Hand { get; }
    TimerService Timer { get; }
    RandomEx Random { get; }
}
