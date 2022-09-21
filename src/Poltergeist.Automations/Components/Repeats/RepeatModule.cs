using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Services;

namespace Poltergeist.Components.Loops;

public class RepeatModule : MacroModule
{
    public RepeatOptions Options { get; set; } = new();

    public override void OnMacroInitializing(IMacroInitializer macro)
    {
        macro.Environments.Add(new("TotalRepeatCount", 0));

        macro.UserOptions.Add(new OptionItem<bool>(RepeatService.ConfigEnableKey, true)
        {
            DisplayLabel = "Enable",
            Category = "Repeat",
        });

        if (Options.UseCount)
        {
            macro.UserOptions.Add(new OptionItem<int>(RepeatService.ConfigTimesKey, 1)
            {
                DisplayLabel = "Times",
                Category = "Repeat",
            });
        }

        if (Options.UsePersistence)
        {
            macro.UserOptions.Add(new OptionItem<TimeSpan>(RepeatService.ConfigPersistenceKey)
            {
                DisplayLabel = "Persistence",
                Category = "Repeat",
            });
        }

    }

    public override void OnServiceConfiguring(MacroServiceCollection services)
    {
        if (Options != null)
        {
            services.Configure<RepeatOptions>(options =>
            {
                options.AllowInfinityLoop = Options.AllowInfinityLoop;
                options.UseCount = Options.UseCount;
                options.UsePersistence = Options.UsePersistence;
                options.StopOnError = Options.StopOnError;
                options.Instrument = Options.Instrument;
            });
        }
        services.AddAutoload<RepeatService>();
    }

}
