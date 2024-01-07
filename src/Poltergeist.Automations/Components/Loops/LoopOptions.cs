using Poltergeist.Automations.Attributes;

namespace Poltergeist.Automations.Components.Loops;

public class LoopOptions
{
    [SafetyLock]
    public bool IsInfiniteLoopable { get; set; }

    [SafetyLock]
    public int MaxIterationLimit { get; set; } = 100;

    public bool IsCountLimitable { get; set; } = true;
    public bool IsDurationLimitable { get; set; }
    public bool ExcludesIncompleteIteration { get; set; }

    public bool ContinuesOnError { get; set; }

    public int DefaultCount { get; set; } = 1;
    public TimeOnly DefaultDuration { get; set; }

    public LoopInstrumentType Instrument { get; set; }
}
