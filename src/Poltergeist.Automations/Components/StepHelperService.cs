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
    public string Title { get; set; }
    public int Interval { get; set; }
    private List<StepItem> Steps = new();
    private ListInstrument Instrument;

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

    public void Show()
    {
        var instruments = Processor.GetService<InstrumentService>();
        Instrument = instruments.Create<ListInstrument>(inst =>
        {
            inst.Title = Title ?? "Steps:";
        });

        foreach (var item in Steps)
        {
            Instrument.Add(new()
            {
                Status = ProgressStatus.Idle,
                Text = item.Text,
            });
        }
    }

    public void Execute()
    {
        if (Instrument == null)
        {
            Show();
        }

        var timer = new Stopwatch();

        for (var i = 0; i < Steps.Count; i++)
        {
            Instrument.Update(i, new()
            {
                Status = ProgressStatus.Busy,
                Text = Steps[i].Text,
            });

            var success = false;
            timer.Restart();
            try
            {
                Steps[i].Action.Invoke();
                success = true;
            }
            catch (Exception e)
            {
                success = false;
                Logger.Error(e.Message);
            }
            timer.Stop();

            Instrument.Update(i, new()
            {
                Status = success ? ProgressStatus.Succeeded : ProgressStatus.Failed,
                Text = Steps[i].Text,
                Subtext = $"{timer.ElapsedMilliseconds}ms"
            });

            if(Interval > 0 && i < Steps.Count - 1)
            {
                Thread.Sleep(Interval);
            }

            if (!success) break;
        }
    }

}

public class StepItem
{
    public string Text { get; set; }
    public Action Action { get; set; }
}
