using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Modules;

public abstract class MacroModule
{
    public string Name => GetType().Name;

    public virtual bool Validate(IMacroProcessorInformation processor)
    {
        return true;
    }

    public virtual void OnMacroInitialized(IMacroInformation macro)
    {
    }

    public virtual void RegisterServices(IServiceCollection services, RegisterServicesArguments arguments)
    {
    }
}
