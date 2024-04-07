namespace Poltergeist.Automations.Components.Debugger;

[AttributeUsage(AttributeTargets.Method)]
public class DebugMethodAttribute : Attribute
{
    public bool PreventsStart { get; set; }

    public DebugMethodAttribute()
    {
    }

    public DebugMethodAttribute(bool preventsStart)
    {
        PreventsStart = preventsStart;
    }

}