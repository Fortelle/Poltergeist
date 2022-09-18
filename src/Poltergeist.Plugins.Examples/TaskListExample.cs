using System;
using System.Threading;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Instruments;
using Poltergeist.Automations.Processors;
using System.Diagnostics;
using System.Linq;

namespace Poltergeist.Plugins.Examples;

public class TaskListExample : BasicMacro
{
    private const int Count = 4;
    private static readonly int[] Primes = new int[] { 2, 3, 5, 7 };

    public TaskListExample() : base("tasklist_example")
    {
        Title = "Tasklist Example";
        Description = "Creates a task list instrument.";
        Script += ScriptProc;
    }

    private void ScriptProc(MacroProcessor proc)
    {
        var noti = proc.GetService<InstrumentService>();
        var gi = noti.Create<ListInstrument>(inst =>
        {
            inst.Title = "Tasks:";
        });

        for (var i = 0; i < Count; i++)
        {
            gi.Add(new()
            {
                Status = ProgressStatus.Idle,
                Text = $"Task {i + 1}"
            });
        }

        var random = new Random();

        for (var i = 0; i < Count * 2; i++)
        {
            var i2 = i;
            var taskname = i2 < Count ? "Task" : "Extra task";

            gi.Update(i2, new()
            {
                Status = ProgressStatus.Busy,
                Text = $"{taskname} {i2 + 1}: Running..."
            });

            var timer = Stopwatch.StartNew();
            var ms = random.Next(50, 1500);
            Thread.Sleep(ms);
            timer.Stop();

            var result = Primes.Contains(i2);
            var status = result ? ProgressStatus.Failed : ProgressStatus.Succeeded;

            gi.Update(i2, new()
            {
                Status = status,
                Text = $"{taskname} {i2 + 1}: {status}",
                Subtext = $"{timer.ElapsedMilliseconds}ms"
            });
        }

    }

}
