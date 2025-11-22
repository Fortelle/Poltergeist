using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.Inputing;

public class KeyboardInputOptions : InputOptions
{
    public TimeSpanRange? KeyDownUpInterval { get; set; }
    public TimeSpanRange? PressInterval { get; set; }
    public KeyboardInputMode? Mode { get; set; }
}
