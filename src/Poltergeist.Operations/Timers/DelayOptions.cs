using Poltergeist.Automations.Utilities.Maths;

namespace Poltergeist.Operations.Timers;

public class DelayOptions : InputOptions
{
    public bool? Floating { get; set; }
    public (double, double)? FloatingRange { get; set; }

    public bool? ApplyMinimalDelay { get; set; }

    public RangeDistributionType? FloatDistribution { get; set; }
    public RangeDistributionType? RangeDistribution { get; set; }
}
