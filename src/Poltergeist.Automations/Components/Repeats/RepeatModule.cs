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

    public override void OnMacroInitialize(IMacroInitializer macro)
    {
        macro.Statistics.Add(new("TotalRepeatCount", 0));

        macro.UserOptions.Add(new OptionItem<bool>(RepeatService.ConfigEnableKey, true)
        {
            DisplayLabel = "Enable",
            Category = "Repeat",
        });

        if (Options.UseCount)
        {
            macro.UserOptions.Add(new OptionItem<int>(RepeatService.ConfigCountKey, 1)
            {
                DisplayLabel = "Count",
                Category = "Repeat",
            });
        }

        // todo: timespan
        if (Options.UseTimeout)
        {
            macro.UserOptions.Add(new OptionItem<int>(RepeatService.ConfigTimeoutKey)
            {
                DisplayLabel = "Timeout (seconds)",
                Category = "Repeat",
            });
        }

    }

    public override void OnMacroConfigure(MacroServiceCollection services)
    {
        if (Options != null)
        {
            services.Configure<RepeatOptions>(options =>
            {
                options.AllowInfiniteLoop = Options.AllowInfiniteLoop;
                options.UseCount = Options.UseCount;
                options.UseTimeout = Options.UseTimeout;
                options.StopOnError = Options.StopOnError;
                options.Instrument = Options.Instrument;
            });
        }
        services.AddAutoload<RepeatService>();
    }

}
