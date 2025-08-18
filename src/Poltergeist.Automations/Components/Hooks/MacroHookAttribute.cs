namespace Poltergeist.Automations.Components.Hooks;

[AttributeUsage(AttributeTargets.Method)]
public class MacroHookAttribute : Attribute
{
    public bool Once { get; set; }
    public int Priority { get; set; }
}
