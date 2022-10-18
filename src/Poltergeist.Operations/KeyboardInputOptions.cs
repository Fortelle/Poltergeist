namespace Poltergeist.Operations;

public class KeyboardInputOptions : InputOptions
{
    public (int Min, int Max)? PressTime { get; set; }
    public (int Min, int Max)? PressInterval { get; set; }
}
