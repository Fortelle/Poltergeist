using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Components.FlowBuilders;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Test;

public partial class ExampleGroup
{

    [AutoLoad]
    public BasicMacro FlowBuilderServiceExample = new("example_flowbuilderservice")
    {
        Title = "FlowBuilderService Example",
        Description = "This example shows how to create flow executors by using the FlowBuilderService.",

        Execution = (args) =>
        {

            var flow1 = args.Processor.GetService<FlowBuilderService>();
            flow1.Title = "Flow 1: Simple flow";
            flow1.Interval = 500;
            flow1.SubtextType = FlowBuilderSubtextType.Status;
            flow1.Add("Step 1", _ =>
            {
                Thread.Sleep(1000);
            });
            flow1.Add("Step 2", _ =>
            {
                Thread.Sleep(1000);
            });
            flow1.Add("Step 3", _ =>
            {
                Thread.Sleep(1000);
            });
            flow1.CreateInstrument();


            var flow2 = args.Processor.GetService<FlowBuilderService>();
            flow2.Title = "Flow 2: Custom text";
            flow2.Interval = 500;
            flow2.Add(new()
            {
                IdleText = "Idle Text",
                StartingText = "Starting Text",
                EndingText = "Ending Text",
                Subtext = "Subtext",
                Execution = _ =>
                {
                    Thread.Sleep(1000);
                }
            });
            flow2.Add(new()
            {
                IdleText = "Idle Text",
                StartingText = "Starting Text",
                EndingText = "Ending Text",
                Subtext = "Subtext",
                ErrorText = "Error Text",
                Execution = _ =>
                {
                    Thread.Sleep(1000);
                    throw new Exception();
                }
            });
            flow2.Add(new()
            {
                IdleText = "Idle Text",
                StartingText = "Starting Text",
                EndingText = "Ending Text",
                Subtext = "Subtext",
                Execution = _ =>
                {
                    Thread.Sleep(1000);
                }
            });
            flow2.CreateInstrument();


            var flow3 = args.Processor.GetService<FlowBuilderService>();
            flow3.Title = "Flow 3: Progress";
            flow3.Interval = 500;
            flow3.Add("Counter", e =>
            {
                for (var i = 0; i < 10; i++)
                {
                    e.Current = i;
                    Thread.Sleep(300);
                }
            });
            flow3.Add("Percentage", e =>
            {
                e.Max = 1;
                for (var i = 0; i < 10; i++)
                {
                    e.Current = i / 10d;
                    Thread.Sleep(300);
                }
                e.Current = 10 / 10d;
            });
            flow3.Add("Fraction", e =>
            {
                e.Max = 10;
                for (var i = 0; i < 10; i++)
                {
                    e.Current = i;
                    Thread.Sleep(300);
                }
                e.Current = 10;
            });
            flow3.CreateInstrument();


            flow1.Execute();
            flow2.Execute();
            flow3.Execute();
        }

    };

}
