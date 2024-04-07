using Poltergeist.Automations.Macros;

namespace Poltergeist.Test;

public class UnrunnableMacro : MacroBase
{
    public UnrunnableMacro(string name) : base(name)
    {
        IsSingleton = true;
    }

    protected override string? OnValidating()
    {
        return $"This macro is unable to run.";
    }
}
