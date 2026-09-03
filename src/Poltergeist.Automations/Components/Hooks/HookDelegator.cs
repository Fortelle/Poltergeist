namespace Poltergeist.Automations.Components.Hooks;

public class HookDelegator(string key)
{
    public string Key => key;

    public List<HookListener> Listeners { get; } = new();
}
