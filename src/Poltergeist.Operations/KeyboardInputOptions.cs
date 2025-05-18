using Poltergeist.Operations.Foreground;

namespace Poltergeist.Operations;

public class KeyboardInputOptions : InputOptions
{
    public (int Min, int Max)? PressDuration { get; set; }
    public (int Min, int Max)? PressInterval { get; set; }
    public KeyboardInputMode? Mode { get; set; }
}
