namespace Poltergeist.Automations.Components.Hooks;

public class HookListener(Type type, Delegate callback)
{
    public Type Type => type;

    public Delegate Callback => callback;

    public bool Once { get; set; }

    public int Priority { get; set; }

    public string? Subscriber { get; set; }
}
