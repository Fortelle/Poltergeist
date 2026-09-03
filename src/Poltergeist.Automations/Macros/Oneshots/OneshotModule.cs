using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Macros.Oneshots;

public class OneshotModule : MacroModule
{
    public override void RegisterServices(IServiceCollection services, RegisterServicesArguments arguments)
    {
        services.AddSingleton<IWorkflowExecutor, OneshotService>();
    }
}
