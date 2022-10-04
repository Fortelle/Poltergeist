using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Macros;

public abstract class MacroModule
{

    public virtual void OnMacroInitialize(IMacroInitializer macro)
    {
    }

    public virtual void OnMacroConfigure(MacroServiceCollection services, IConfigureProcessor processor)
    {
    }

    public virtual void OnMacroProcess(MacroProcessor processor)
    {
    }

    public virtual void SetGlobalOptions(MacroOptions options)
    {

    }
}
