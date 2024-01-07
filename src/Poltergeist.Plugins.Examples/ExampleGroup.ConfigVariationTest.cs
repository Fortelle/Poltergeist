using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Components.Loops;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Parameters;

namespace Poltergeist.Test;

public partial class ExampleGroup
{
    [AutoLoad]
    public LoopMacro ConfigVariationTestMacro = new("example_configvariations")
    {
        Title = "ConfigVariations Example",

        Description = "This example shows how to run macro with different configuration sets.",

        UserOptions =
        {
            new OptionItem<bool>("reset", false),
            new OptionItem<bool>("maintenance_mode", false)
            {
                IsBrowsable = false,
            },
        },

        ConfigVariations =
        {
            new ConfigVariation()
            {
                Title = "Loop 5 times",
                Normalized = true,
                Options = new()
                {
                    {LoopService.ConfigEnableKey, true },
                    {LoopService.ConfigCountKey, 5 },
                },
            },
            new ConfigVariation()
            {
                Title = "Force reset",
                Options = new()
                {
                    {"reset", true },
                },
            },
            new ConfigVariation()
            {
                Title = "Maintenance",
                Glyph = "\uE90F",
                Normalized = true,
                Options = new()
                {
                    {"maintenance_mode", true},
                    {LoopService.ConfigEnableKey, false},
                },
                Environments = new()
                {
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
            new ParameterEntry<int>("counter", 0),
        },

        Before = (LoopBeforeArguments args) =>
        {
            if (args.Processor.Options.Get<bool>("maintenance_mode"))
            {
                var counter = args.Processor.Statistics.Get<int>("counter");

                args.Outputer.NewGroup("Maintenance Mode");
                args.Outputer.Write($"Current number is {counter}.");
                args.Cancel = true;
                return;
            }

            if (args.Processor.Options.Get<bool>("reset"))
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
