using System.Diagnostics;
using System.Threading;
using Poltergeist.Automations.Macros;
using Poltergeist.Operations;
using Poltergeist.Operations.ForegroundWindows;
using Poltergeist.Operations.Macros;

namespace Poltergeist.Test;

public class OperationGroup : MacroGroup
{
    public OperationGroup() : base("OperationTests")
    {
        LoadMacros();
    }

    public void LoadMacros()
    {
        Macros.Add(new BasicMacro("example_foreground_module")
        {
            Title = "Foreground module",
            Description = "Injects foreground module into a basic macro.",
            Modules =
            {
                new ForegroundModule(),
                new InputOptionsModule(),
            },
            Script = (e) =>
            {
                var ope = e.Processor.GetService<ForegroundOperator>();

                using var process = new Process()
                {
                    StartInfo =
                    {
                        FileName = "notepad",
                        UseShellExecute = true,
                    },
                };
                process.Start();
                ope.Timer.Delay(500);

                var result = ope.Locating.Locate(new()
                {
                    Handle = process.MainWindowHandle,
                    BringToFront = true,
                });
                if (!result) return;

                ope.Keyboard.Input("Hello world!");
            },
        });

        Macros.Add(new ForegroundMacro("example_foreground_macro")
        {
            Title = "Foreground macro",
            Description = "An example of ForegroundMacro.",

            Filename = "notepad",

            RegionConfig = new()
            {
                BringToFront = true,
            },

            Iteration = (e, ope) =>
            {
                ope.Keyboard.Input("Hello world!");
            },

        });

    }

}
