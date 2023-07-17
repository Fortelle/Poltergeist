using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Operations.BackgroundWindows;

public class BackgroundModule : MacroModule
{
    public override void OnMacroConfiguring(ServiceCollection services, IConfigureProcessor processor)
    {
        services.AddScoped<BackgroundLocatingService>();
        services.AddScoped<BackgroundCapturingService>();
    }
}
