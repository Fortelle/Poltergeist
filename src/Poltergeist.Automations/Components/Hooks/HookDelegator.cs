namespace Poltergeist.Automations.Components.Hooks;

public class HookDelegator
{
    public Type Type { get; }

    public string Name { get; }

    public List<HookListener> Listeners { get; } = new();

    public HookDelegator(Type hookType)
    {
        Type = hookType;

        var name = hookType.Name;
        if (name.EndsWith("Hook"))
        {
            name = name[0..^4];
        }
        Name = name;
    }
}
