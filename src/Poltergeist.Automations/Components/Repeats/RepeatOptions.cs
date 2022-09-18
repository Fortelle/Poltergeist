using Poltergeist.Automations.Attributes;

namespace Poltergeist.Components.Loops;

public class RepeatOptions
{
    [SafetyLock]
    public bool AllowInfinityLoop { get; set; }

    public bool UseCount { get; set; } = true;
    public bool UsePersistence { get; set; }

    public bool StopOnError { get; set; } = true;

    public RepeatInstrumentType Instrument { get; set; }
}
