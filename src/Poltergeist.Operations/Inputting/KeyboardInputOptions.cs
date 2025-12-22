using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.Inputting;

public class KeyboardInputOptions : InputOptions
{
    public TimeSpanRange? KeyDownUpInterval { get; set; }
    public TimeSpanRange? PressInterval { get; set; }
    public KeyboardInputMode? Mode { get; set; }
}
