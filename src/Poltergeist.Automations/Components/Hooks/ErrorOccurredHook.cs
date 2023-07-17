namespace Poltergeist.Automations.Components.Hooks;

public class ErrorOccurredHook : MacroHook
{
    public string Message { get; set; }

    public ErrorOccurredHook(string message)
    {
        Message = message;
    }
}
