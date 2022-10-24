using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Poltergeist.Automations.Components;
using Poltergeist.Automations.Configs;
using Poltergeist.Automations.Macros;
using Poltergeist.Input.Windows;

namespace Poltergeist.Plugins.Examples;

public class ExampleMacroGroup : MacroGroup
{
    public ExampleMacroGroup() : base("Examples")
    {
        Description = "Provides a series of examples.";
        LoadMacros();
    }

    public void LoadMacros()
    {
        Macros.Add(new BasicMacro("hello_world")
        {
            Title = "Hello world",
            Description = "Opens a notepad and inputs text.",
            Configure = (services, _) =>
            {
                services.AddTransient<StepHelperService>();
            },
            Script = (e) =>
            {
                Process notepad = null;
                
                var steps = e.Processor.GetService<StepHelperService>();
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
        });

        Macros.Add(new GridExample());

        Macros.Add(new TaskListExample());

        Macros.Add(new BasicMacro("test_stephelper")
        {
            Title = "StepHelper",
            Description = "Tests StepHelper.",
            ShowStatus = false,
            UserOptions =
            {
                new OptionItem<bool>("break", false),
            },
            Configure = (services, _) =>
            {
                services.AddTransient<StepHelperService>();
            },
            Script = (e) =>
            {
                var steps1 = e.Processor.GetService<StepHelperService>();
                steps1.Title = "Section 1:";
                steps1.Add("Say Hello", () =>
                {
                    Thread.Sleep(500);
                });
                steps1.Add("Say World", () =>
                {
                    Thread.Sleep(500);
                    if (e.Processor.GetOption("break", false)) throw new Exception();
                });
                steps1.Add("Done", () =>
                {
                    Thread.Sleep(500);
                });
                steps1.Show();

                var steps2 = e.Processor.GetService<StepHelperService>();
                steps2.Title = "Section 2:";
                steps2.Interval = 500;
                steps2.Add("Say Hello", () => { });
                steps2.Add("Say World", () => { });
                steps2.Add("Done", () => { });
                steps2.Show();

                steps1.Execute();
                steps2.Execute();
            }
        });
    }

}
