namespace Poltergeist.Automations.Components.Hooks;

public class HookListener(string key, Delegate callback)
{
    public string Key => key;

    public Delegate Callback => callback;

    public bool Once { get; set; }

    public int Priority { get; set; }

    public string? Subscriber { get; set; }

    public string? MethodName { get; set; }
}