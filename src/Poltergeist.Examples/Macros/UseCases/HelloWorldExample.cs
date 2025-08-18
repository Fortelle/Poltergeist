using System.Diagnostics;
using Poltergeist.Automations.Components.FlowBuilders;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Utilities.Windows;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class HelloWorldExample : BasicMacro
{
    public HelloWorldExample() : base()
    {
        Title = "Hello World";

        Category = "Use Cases";

        Description = "This example opens a notepad.exe window and inputs text into it.";

        Execute = (args) =>
        {
            Process? notepad = null;

            var flow = args.Processor.GetService<FlowBuilderService>();
            flow.Interval = 1000;

            flow.Add("Open a new notepad window", e =>
            {
                notepad = Process.Start("notepad.exe");
            });

            flow.Add("Input text \"Hello world!\"", e =>
            {
                SendInputHelper.Send("Hello world!", 100);
                SendInputHelper.KeyPress(VirtualKey.Return);
            });

            flow.Add("Close notepad window without saving", e =>
            {
                //notepad.CloseMainWindow(); // not working in win11
                SendInputHelper.KeyPress(VirtualKey.Control, VirtualKey.W);

                Thread.Sleep(1000);

                SendInputHelper.KeyPress(VirtualKey.N);
            });

            flow.Execute();
        };
    }
};
