using Poltergeist.Automations.Macros;

namespace Poltergeist.Test;

public class UnrunnableMacro : MacroBase
{
    public UnrunnableMacro(string name) : base(name)
    {
    }

    public override string? CheckValidity()
    {
        return $"This macro is unable to run.";
    }
}
