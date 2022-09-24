using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components;
using Poltergeist.Automations.Components.Repeats;
using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Logging;
using Poltergeist.Automations.Macros;
using Poltergeist.Components.Loops;

namespace Poltergeist.Test;

public class TestMacroGroup : MacroGroup
{
    public TestMacroGroup() : base("Tests")
    {
        Description = "Provides a series of test macros.";
        Options = new()
        {
            new OptionItem<int>("test_option_a", 1),
            new OptionItem<int>("test_option_c", 1),
        };
        LoadMacros();
    }

    public override void SetGlobalOptions(MacroOptions options)
    {
        options.Add<int>("test_option_a", 0);
        options.Add<int>("test_option_b", 0);
    }

    public void LoadMacros()
    {
        Macros.Add(new MinimalMacro("test_minimal")
        {
            Title = "Minimal",
            Description = "Provides an empty macro with the minimum dependencies.",
        });

        Macros.Add(new ExceptionMacro("test_exception")
        {
            Title = "Exception test",
            Description = "Throws an exception in different stages.",
        });

        Macros.Add(new BasicMacro("test_log")
        {
            Title = "Log test",
            Description = "Tests logger.",
            Script = (proc) =>
            {
                var logger = proc.GetService<MacroLogger>();
                foreach (var level in Enum.GetValues<LogLevel>())
                {
                    logger.Log(level, "", $"Test log line: {{{level}}}.");
                }
            }
        });

        Macros.Add(new BasicMacro("test_config")
        {
            Title = "Config test",
            Description = "",
            UserOptions =
            {
                new OptionItem<bool>("bool"),
                new OptionItem<int>("int"),
                new OptionItem<double>("double"),
                new OptionItem<string>("string"),

                new ChoiceOptionItem<string>("choice_string", new string[] { "Item1", "Item2", "Item3" }, "Item1"),
                new ChoiceOptionItem<int>("choice_int", new int[] { 100, 200, 300 }, 100),
                new FileOptionItem("file"),

                new OptionItem<int>("test_option_a", 2),
                new OptionItem<int>("test_option_d", 2),
            },
            Script = (proc) =>
            {
            }
        });

        Macros.Add(new RepeatableMacro("test_repeat")
        {
            Title = "RepeatableMacro",
            Description = "Provides a RepeatableMacro.",
            UserOptions =
            {
                //new(RepeatService.ConfigTimesKey, 10)
                //{
                //    DisplayLabel = "Times",
                //    Category = "Repeat",
                //},
                //new("loop-instrument", RepeatInstrumentType.List)
                //{
                //    DisplayLabel = "Instrument",
                //    Category = "Repeat",
                //},
            },
            Configure = (services) =>
            {
                services.Configure<RepeatOptions>(options =>
                {
                    options.Instrument = RepeatInstrumentType.Grid;
                });
            },
            Iteration = (proc) =>
            {
                Thread.Sleep(500);
                return IterationResult.Continue;
            }
        });
    }

}
