using Poltergeist.Automations.Macros;

namespace Poltergeist.Examples.Macros;

public class UnrunnableMacro : MacroBase
{
    public UnrunnableMacro() : base()
    {
    }

    protected override bool OnValidating(out string invalidationMessage)
    {
        invalidationMessage = $"This macro is unable to run.";
        return false;
    }
}
