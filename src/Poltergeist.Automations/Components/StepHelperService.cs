using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Poltergeist.Automations.Instruments;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Components;

public class StepHelperService : MacroService
{
    public int Interval { get; set; }
    private List<StepItem> Steps = new();

    public StepHelperService(MacroProcessor proc) : base(proc)
    {
    }

    public void Add(string text, Action action)
    {
        Steps.Add(new()
        {
            Text = text,
            Action = action
        });
    }

    public void Execute()
    {
        var noti = Processor.GetService<InstrumentService>();
        var gi = noti.Create<ListInstrument>(inst =>
        {
            inst.Title = "Steps:";
        });

        foreach (var item in Steps)
        {
            gi.Add(new()
            {
                Status = ProgressStatus.Idle,
                Text = item.Text,
            });
        }

        var timer = new Stopwatch();

        for (var i = 0; i < Steps.Count; i++)
        {
            gi.Update(i, new()
            {
                Status = ProgressStatus.Busy,
                Text = Steps[i].Text,
            });

            var sucessed = false;

            timer.Restart();
            try
            {
                Steps[i].Action.Invoke();
                sucessed = true;
            }
            catch (Exception)
            {
                sucessed = false;
            }
            timer.Stop();

            gi.Update(i, new()
            {
                Status = sucessed ? ProgressStatus.Succeeded : ProgressStatus.Failed,
                Text = Steps[i].Text,
                Subtext = $"{timer.ElapsedMilliseconds}ms"
            });

            if(Interval > 0 && i < Steps.Count - 1)
            {
                Thread.Sleep(Interval);
            }

            if (!sucessed) break;
        }
    }

}

public class StepItem
{
    public string Text { get; set; }
    public Action Action { get; set; }
}
