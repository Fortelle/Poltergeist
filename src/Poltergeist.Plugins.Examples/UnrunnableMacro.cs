using Poltergeist.Automations.Macros;

namespace Poltergeist.Test;

public class UnrunnableMacro : MacroBase
{
    public UnrunnableMacro(string name) : base(name)
    {
        IsSingleton = true;
    }

    protected override bool OnValidating(out string invalidationMessage)
    {
        invalidationMessage = $"This macro is unable to run.";
        return false;
    }
}
