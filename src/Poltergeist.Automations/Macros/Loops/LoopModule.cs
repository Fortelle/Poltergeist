using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Modules;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Macros.Loops;

public class LoopModule : MacroModule
{
    public override void RegisterServices(IServiceCollection services, RegisterServicesArguments arguments)
    {
        services.AddSingleton<IWorkflowExecutor, LoopService>();
    }
}
