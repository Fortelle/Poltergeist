using Poltergeist.Automations.Modules;

namespace Poltergeist.Automations.Macros;

[ModuleDependency<CommonConfiguralizationModule>]
public abstract class CommonMacroBase : MacroBase
{
    protected CommonMacroBase(string? name = null) : base(name)
    {
    }
}
