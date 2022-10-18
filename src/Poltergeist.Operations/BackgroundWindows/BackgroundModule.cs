using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Operations.BackgroundWindows;

public class BackgroundModule : MacroModule
{
    public override void OnMacroConfigure(MacroServiceCollection services, IConfigureProcessor processor)
    {
        services.AddScoped<BackgroundLocatingService>();
        services.AddScoped<BackgroundCapturingService>();
    }
}
