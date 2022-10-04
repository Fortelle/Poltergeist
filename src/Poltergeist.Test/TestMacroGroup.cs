using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components.Repeats;
using Poltergeist.Automations.Components.Terminals;
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
            Script = (e) =>
            {
                var logger = e.Processor.GetService<MacroLogger>();
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

                new OptionItem<string>("readonly", "this option can not be changed")
                {
                    IsReadonly = true
                },
                new UndefinedOptionItem("undefined", "undefined"),

                new OptionItem<string>("unbrowsable", "this option should not be seen")
                {
                    IsBrowsable = false
                },
            },
            Script = (proc) =>
            {
                var a = 0;
            }
        });

        Macros.Add(new RepeatableMacro("test_repeat")
        {
            Title = "RepeatableMacro",
            Description = "Provides a RepeatableMacro.",
            UserOptions =
            {
            },
            Configure = (services, _) =>
            {
                services.Configure<RepeatOptions>(options =>
                {
                    options.Instrument = RepeatInstrumentType.Grid;
                });
            },
            Iteration = (e) =>
            {
                Thread.Sleep(500);
            }
        });

        Macros.Add(new BasicMacro("test_cmd")
        {
            Configure = (services, _) =>
            {
                services.AddSingleton<TerminalService>();
            },
            Script = (e) =>
            {
                var cmd = e.Processor.GetService<TerminalService>();
                cmd.Start();
                Thread.Sleep(100);
                cmd.Execute("cd");
                cmd.Execute("cd /d c:/");
                Thread.Sleep(100);
                cmd.Execute("cd");
                cmd.Execute("echo hello");
                cmd.Close();
            }
        });

    }

}
