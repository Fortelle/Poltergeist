using System.Diagnostics;
using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Components.FlowBuilders;
using Poltergeist.Automations.Macros;
using Poltergeist.Input.Windows;

namespace Poltergeist.Test;

public partial class ExampleGroup : MacroGroup
{

    [AutoLoad]
    public BasicMacro HelloWorldExample = new("example_helloworld")
    {
        Title = "Hello World",

        Description = "Opens a notepad and inputs text.",

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
        }
    };

}
