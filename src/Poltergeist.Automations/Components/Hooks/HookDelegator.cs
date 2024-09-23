namespace Poltergeist.Automations.Components.Hooks;

public class HookDelegator
{
    public Type Type { get; }

    public string Name { get; }

    public List<HookListener> Listeners { get; } = new();

    public HookDelegator(Type hookType)
    {
        Type = hookType;
        Name = hookType.Name.Replace("Hook", null); ;
    }
}
