using Poltergeist.Automations.Components.FlowBuilders;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class FlowBuilderServiceExample : BasicMacro
{
    public FlowBuilderServiceExample() : base()
    {
        Title = nameof(FlowBuilderService);

        Category = "Features";

        Description = $"This example uses the {nameof(FlowBuilderService)} to create flow executors.";

        Execute = (args) =>
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
                var max = 10;
                for (var i = 0; i < max; i++)
                {
                    Thread.Sleep(300);
                    e.Update(new()
                    {
                        ProgressValue = i + 1,
                        ProgressMax = max,
                        Subtext = $"{i + 1}/{max}",
                    });
                }
            });
            flow3.Add("Percentage", e =>
            {
                var max = 10;
                for (var i = 0; i < max; i++)
                {
                    Thread.Sleep(300);
                    e.Update(new()
                    {
                        ProgressValue = i + 1,
                        ProgressMax = max,
                        Subtext = $"{(i + 1d) / max:#%}",
                    });
                }
            });
            flow3.CreateInstrument();


            flow1.Execute();
            Thread.Sleep(1000);
            flow2.Execute();
            Thread.Sleep(1000);
            flow3.Execute();
        };
    }
}
