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

        IsSingleton = true,

        ExecuteAsync = async (args) =>
        {
            var count = 50;
            var rnd = new Random();

            var gi = args.Processor.GetService<DashboardService>().Create<ProgressGridInstrument>(gi =>
            {
                gi.Title = "Tasks:";
                gi.AddPlaceholders(count, new(ProgressStatus.Idle));
            });

            var options = new ParallelOptions {
                CancellationToken = (CancellationToken)args.Processor.CancellationToken,
                MaxDegreeOfParallelism = 5,
            };
            await Parallel.ForEachAsync(Enumerable.Range(0, count), options, async (i, c) =>
            {
                args.Logger.Log(i.ToString());
                gi.Update(i, new(ProgressStatus.Busy));

                await Task.Delay(rnd.Next(500, 5000), c);

                if (args.Processor.IsCancelled)
                {
                    return;
                }

                var result = rnd.NextDouble() < .8 ? ProgressStatus.Success : ProgressStatus.Failure;
                gi.Update(i, new(result));
            });
        }

    };

}
