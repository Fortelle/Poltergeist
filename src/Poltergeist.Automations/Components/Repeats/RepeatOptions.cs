using Poltergeist.Automations.Attributes;

namespace Poltergeist.Components.Loops;

public class RepeatOptions
{
    [SafetyLock]
    public bool AllowInfiniteLoop { get; set; }

    public bool UseCount { get; set; } = true;
    public bool UseTimeout { get; set; }

    public bool StopOnError { get; set; } = true;

    public RepeatInstrumentType Instrument { get; set; }
}
