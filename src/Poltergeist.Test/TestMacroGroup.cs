using System;
using System.Media;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components;
using Poltergeist.Automations.Components.Repeats;
using Poltergeist.Automations.Components.Terminals;
using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Instruments;
using Poltergeist.Automations.Logging;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;
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

        Macros.Add(new BasicMacro("test_sound")
        {
            Title = "Sound test",
            Configure = (services, _) =>
            {
                services.AddTransient<StepHelperService>();
                services.AddSingleton<SoundService>();
            },
            Script = (e) =>
            {
                var sounds = e.Processor.GetService<SoundService>();

                var sh1 = e.Processor.GetService<StepHelperService>();
                sh1.Title = "Play system sounds:";
                sh1.Interval = 1000;
                sh1.Add("Play Beep", () => sounds.Beep());
                sh1.Add("Play Asterisk", () => sounds.Asterisk());
                sh1.Add("Play Exclamation", () => sounds.Exclamation());
                sh1.Add("Play Hand", () => sounds.Hand());
                sh1.Add("Play Question", () => sounds.Question());
                sh1.Show();

                sh1.Execute();
            }
        });

        //Macros.Add(new BasicMacro("test_bgm")
        //{
        //    // todo: ignore fadeout on error
        //    // todo: use progress instrument
        //    Title = "BGM test",
        //    Description = "Plays background music while macro is running.",
        //    UserOptions =
        //    {
        //        new FileOptionItem("bgm"),
        //        new OptionItem<int>("duration", "duration (ms)", 10000),
        //        new OptionItem<int>("fadeout", "fadeout (ms)", 3000),
        //    },
        //    Configure = (services, _) =>
        //    {
        //        services.AddSingleton<SoundService>();
        //    },
        //    Process = (e) =>
        //    {
        //        var bgmfile = e.GetOption<string>("bgm");
        //        if (string.IsNullOrEmpty(bgmfile))
        //        {
        //            return;
        //        }
        //        var fadeout = e.GetOption<int>("fadeout");

        //        var sounds = e.GetService<SoundService>();
        //        var hooks = e.GetService<HookService>();
        //        hooks.Register("process_started", _ => sounds.PlayBgm(bgmfile));
        //        hooks.Register("process_exiting", _ => sounds.StopBgm(fadeout));
        //    },
        //    Script = (e) =>
        //    {
        //        var duration = e.Processor.GetOption<int>("duration");
        //        var sounds = e.Processor.GetService<SoundService>();
        //        Thread.Sleep(duration >> 1);
        //        sounds.Beep();
        //        Thread.Sleep(duration >> 1);
        //    }
        //});

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

        Macros.Add(new BasicMacro("test_minimize")
        {
            MinimizeApplication = true,
            Script = (e) =>
            {
                Thread.Sleep(3000);
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
