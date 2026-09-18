using System.Diagnostics.CodeAnalysis;

namespace Poltergeist.Automations.Structures;

public class FrameAnimation : IconAnimation
{
    public bool AutoReverse { get; set; }

    public int Count { get; set; }

    public TimeSpan Interval { get; set; } = TimeSpan.FromMilliseconds(100);

    public required string[] Values { get; set; }

    public FrameAnimation()
    {

    }

    [SetsRequiredMembers]
    public FrameAnimation(params string[] values)
    {
        Values = values;
    }
}
