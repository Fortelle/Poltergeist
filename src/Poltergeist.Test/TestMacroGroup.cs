using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components;
using Poltergeist.Automations.Components.Repeats;
using Poltergeist.Automations.Logging;
using Poltergeist.Automations.Macros;
using Poltergeist.Components.Loops;
using Poltergeist.Plugins;

namespace Poltergeist.Test;

public class TestMacroGroup : IMacroGroup
{
    public string Name => "Tests";

    public string Description => "Provides a series of test macros.";

    public IEnumerable<MacroBase> GetMacros()
    {
        yield return new MinimalMacro("test_minimal")
        {
            Title = "Minimal",
            Description = "Provides an empty macro with the minimum dependencies.",
        };

        yield return new ExceptionMacro("test_exception")
        {
            Title = "Exception test",
            Description = "Throws an exception in different stages.",
        };

        yield return new BasicMacro("test_log")
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
        };

        yield return new RepeatableMacro("test_repeat")
        {
            Title = "RepeatableMacro",
            Description = "Provides a RepeatableMacro.",
            UserOptions =
            {
                new(RepeatService.ConfigTimesKey, 10)
                {
                    DisplayLabel = "Times",
                    Category = "Repeat",
                },
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
        };

    }

}
