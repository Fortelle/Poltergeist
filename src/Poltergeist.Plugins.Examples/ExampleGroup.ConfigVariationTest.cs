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
                    {"incognito_mode", true},
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
            if (args.Processor.Environments.GetValueOrDefault<bool>("maintenance_mode"))
            {
                var counter = args.Processor.Statistics.GetValueOrDefault<int>("counter");

                args.Outputer.NewGroup("Maintenance Mode");
                args.Outputer.Write($"Current number is {counter}.");
                args.Cancel = true;
                return;
            }

            if (args.Processor.Environments.GetValueOrDefault<bool>("reset"))
            {
                args.Processor.Statistics.TryRemove("counter");
            }

            args.Outputer.NewGroup($"Counting:");
        },

        Iterate = (IterationArguments args) =>
        {
            var counter = args.Processor.Statistics.GetValueOrDefault<int>("counter");

            counter++;
            Thread.Sleep(100);

            args.Outputer.Write($"{counter}");
            args.Processor.Statistics.AddOrUpdate("counter", counter);
        }
    };
}
