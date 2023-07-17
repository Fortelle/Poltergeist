using Poltergeist.Automations.Attributes;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Test;

public partial class ExampleGroup
{
    [AutoLoad]
    public BasicMacro MultitasksExample = new("example_multitasks")
    {
        Title = "Multitasks Example",
        Description = "This example shows how to create a multi task indicator by using the ProgressGridInstrument.",

        Execution = (args) =>
        {
            var count = 50;
            var rnd = new Random();

            var gi = args.Processor.GetService<DashboardService>().Create<ProgressGridInstrument>(gi =>
            {
                gi.Title = "Tasks:";
                gi.AddPlaceholders(count, new(ProgressStatus.Idle));
            });

            void action(int i)
            {
                gi.Update(i, new(ProgressStatus.Busy));

                Thread.Sleep(rnd.Next(500, 5000));

                if (args.Processor.IsCancelled)
                {
                    return;
                }

                var result = rnd.NextDouble() < .8 ? ProgressStatus.Success : ProgressStatus.Failure;
                gi.Update(i, new(result));
            }

            var tasks = Enumerable.Range(0, count)
                .Select(i => (Action)(() =>
                {
                    action(i);
                }))
                .ToArray();

            var options = new ParallelOptions {
                CancellationToken = (CancellationToken)args.Processor.CancellationToken,
                MaxDegreeOfParallelism = 5,
            };
            Parallel.Invoke(options, tasks);
        }

    };

}
