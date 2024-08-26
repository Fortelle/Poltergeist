using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Test;

public partial class ExampleGroup
{
    [AutoLoad]
    public LoopMacro ConfigVariationTestMacro = new("example_configvariations")
    {
        Title = "ConfigVariations Example",

        Description = "This example shows how to run macro with different configuration sets.",

        IsSingleton = true,

        ConfigVariations =
        {
            new ConfigVariation()
            {
                Title = "Loop 5 times",
                IgnoresUserOptions = true,
                Icon = "\uE895",
                OptionOverrides = new()
                {
                    {LoopService.ConfigEnableKey, true },
                    {LoopService.ConfigCountKey, 5 },
                },
            },
            new ConfigVariation()
            {
                Title = "Force reset",
                EnvironmentOverrides = new()
                {
                    {"reset", true },
                },
            },
            new ConfigVariation()
            {
                Title = "Maintenance",
                Icon = "\uE90F",
                IgnoresUserOptions = true,
                OptionOverrides = new()
                {
                    {LoopService.ConfigEnableKey, false},
                },
                EnvironmentOverrides = new()
                {
                    {"maintenance_mode", true},
                    {MacroBase.UseStatisticsKey, false},
                }
            }
        },

        LoopOptions =
        {
            Instrument = LoopInstrumentType.None,
            DefaultCount = 10,
        },

        Statistics =
        {
            new ParameterDefinition<int>("counter", 0),
        },

        Before = (LoopBeforeArguments args) =>
        {
            if (args.Processor.Environments.Get<bool>("maintenance_mode"))
            {
                var counter = args.Processor.Statistics.Get<int>("counter");

                args.Outputer.NewGroup("Maintenance Mode");
                args.Outputer.Write($"Current number is {counter}.");
                args.Cancel = true;
                return;
            }

            if (args.Processor.Environments.Get<bool>("reset"))
            {
                args.Processor.Statistics.Set("counter", 0);
            }

            args.Outputer.NewGroup($"Counting:");
        },

        Execute = (LoopExecuteArguments args) =>
        {
            var counter = args.Processor.Statistics.Get<int>("counter");

            counter++;
            Thread.Sleep(100);

            args.Outputer.Write($"{counter}");
            args.Processor.Statistics.Set("counter", counter);
        }
    };
}
