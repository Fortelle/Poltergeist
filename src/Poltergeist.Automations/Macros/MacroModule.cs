using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Macros;

public abstract class MacroModule
{

    public virtual void OnMacroInitialized(IMacroInitializer macro)
    {
    }

    public virtual void OnMacroConfiguring(ServiceCollection services, IConfigureProcessor processor)
    {
    }

    public virtual void OnMacroProcessing(MacroProcessor processor)
    {
    }

    public virtual void SetGlobalOptions(MacroOptions options)
    {

    }
}
