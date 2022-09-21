using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Macros;

public abstract class MacroModule
{

    public virtual void OnMacroInitializing(IMacroInitializer macro)
    {
    }

    public virtual void OnServiceConfiguring(MacroServiceCollection services)
    {
    }

    public virtual void OnProcessorLoading(MacroProcessor processor)
    {
    }

    public virtual void SetGlobalOptions(MacroOptions options)
    {

    }
}
