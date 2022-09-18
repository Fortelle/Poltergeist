using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components;
using Poltergeist.Automations.Macros;
using Poltergeist.Input.Windows;

namespace Poltergeist.Plugins.Examples;

public class ExampleMacroGroup : IMacroGroup
{
    public string Name => "Examples";

    public string Description => "Provides a series of examples.";

    public IEnumerable<MacroBase> GetMacros()
    {
        yield return new BasicMacro("hello_world")
        {
            Title = "Hello world",
            Description = "Opens a notepad and inputs text.",
            Configure = (services) =>
            {
                services.AddTransient<StepHelperService>();
            },
            Script = (proc) =>
            {
                Process notepad = null;

                var steps = proc.GetService<StepHelperService>();
                steps.Interval = 1000;

                steps.Add("Wait 1 second", () =>
                {
                    Thread.Sleep(1000);
                });

                steps.Add("Open a notepad window", () =>
                {
                    notepad = Process.Start("notepad.exe");
                });

                steps.Add("Input \"Hello world!\"", () =>
                {
                    SendInputHelper.Send("Hello world!", 100);
                });

                steps.Add("Close notepad without saving", () =>
                {
                    notepad.CloseMainWindow();

                    Thread.Sleep(1000);

                    SendInputHelper.KeyPress(VirtualKey.N);
                });

                steps.Execute();
            }
        };



        yield return new GridExample();

        yield return new TaskListExample();

        yield return new BasicMacro("test_stephelper")
        {
            Title = "StepHelper",
            Description = "Tests StepHelper.",
            ShowStatus = false,
            UserOptions =
            {
                new OptionItem("break", false),
            },
            Configure = (services) =>
            {
                services.AddSingleton<StepHelperService>();
            },
            Script = (proc) =>
            {
                var steps = proc.GetService<StepHelperService>();
                steps.Add("Say Hello", () =>
                {
                    Thread.Sleep(500);
                });
                steps.Add("Say World", () =>
                {
                    Thread.Sleep(500);
                    if (proc.GetOption("break", false)) throw new Exception();
                });
                steps.Add("Done", () =>
                {
                    Thread.Sleep(500);
                });

                steps.Execute();
            }
        };
    }

}
