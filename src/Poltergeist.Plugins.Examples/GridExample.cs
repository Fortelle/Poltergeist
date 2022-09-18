using System;
using System.Threading;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Instruments;
using System.Linq;
using System.Threading.Tasks;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Plugins.Examples;

public class GridExample : BasicMacro
{
    private const int Count = 100;
    private static readonly int[] Primes = new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97 };

    public GridExample() : base("grid_example")
    {
        Title = "Grid Example";
        Description = "Creates a grid instrument.";
        Script += ScriptProc;

        UserOptions.Add("placeholder", false);
    }

    private void ScriptProc(MacroProcessor proc)
    {
        var usePlaceholder = proc.GetOption("placeholder", false);
        //var gi = usePlaceholder ? new GridInstrument(Count) : new GridInstrument();

        var noti = proc.GetService<InstrumentService>();
        var gi = noti.Create<GridInstrument>(gi =>
        {
            gi.Title = "Task grids:";
            if (usePlaceholder) gi.SetPlaceholders(Count);
        });

        var tasks = Enumerable.Range(0, Count)
            .Select(v => (Action)(() =>
            {
                if (usePlaceholder)
                {
                    gi.Update(v, new(ProgressStatus.Busy));
                }
                else
                {
                    gi.Add(new(ProgressStatus.Busy));
                }

                Thread.Sleep(500);

                var r = Primes.Contains(v) ? ProgressStatus.Failed : ProgressStatus.Succeeded;
                gi.Update(v, new(r));
            }))
            .ToArray();

        var options = new ParallelOptions { MaxDegreeOfParallelism = 5 };
        Parallel.Invoke(options, tasks);
    }

}
