using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components;
using Poltergeist.Automations.Modules;

namespace Poltergeist.Automations.Macros;

public class TriggerModule : MacroModule
{
    public override void RegisterServices(IServiceCollection services, RegisterServicesArguments arguments)
    {
        services.AddTransient<EventTrigger>();
        services.AddTransient<PeriodicTrigger>();
    }
}
