using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class MultitasksExample : BasicMacro
{
    public MultitasksExample() : base()
    {
        Title = "Multitasks Example";

        Category = "Use Cases";

        Description = "This example simulates a multitasking scenario and displays the progress to the dashboard.";

        Execute = (args) =>
        {
            var count = 100;
            var rnd = new Random();

            var gi = args.Processor.GetService<DashboardService>().Create<ProgressTileInstrument>(gi =>
            {
                gi.Title = "Tasks:";
                gi.AddPlaceholders(count, new(ProgressStatus.Idle));
            });

            var data = Enumerable.Range(0, count);
            var options = new ParallelOptions
            {
                CancellationToken = args.Processor.CancellationToken,
                MaxDegreeOfParallelism = 12,
            };

            Parallel.ForEach(data, options, (i, c) =>
            {
                gi.Update(i, new(ProgressStatus.Busy));

                Thread.Sleep(rnd.Next(300, 3000));

                var result = rnd.NextDouble() < .8 ? ProgressStatus.Success : ProgressStatus.Failure;
                gi.Update(i, new(result));
            });
        };
    }
}
