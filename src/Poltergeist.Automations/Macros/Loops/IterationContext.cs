using System.Diagnostics.CodeAnalysis;

namespace Poltergeist.Automations.Macros.Loops;

public class IterationContext
{
    public int Index { get; init; }

    public bool? ShouldBreak { get; set; }

    [DoesNotReturn]
#pragma warning disable CA1822 // Mark members as static
    public void Continue(string? message = null)
#pragma warning restore CA1822 // Mark members as static
    {
        throw new IterationContinueException(message);
    }

    [DoesNotReturn]
#pragma warning disable CA1822 // Mark members as static
    public void Break(string? message = null)
#pragma warning restore CA1822 // Mark members as static
    {
        throw new IterationBreakException(message);
    }
}
