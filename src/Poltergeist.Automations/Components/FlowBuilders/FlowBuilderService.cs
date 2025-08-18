using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Components.FlowBuilders;

public class FlowBuilderService : MacroService
{
    public string? Title { get; set; }
    public int Interval { get; set; }
    public bool? ToastNotification { get; set; }
    public FlowBuilderSubtextType SubtextType { get; set; } = FlowBuilderSubtextType.None;

    private readonly List<FlowBuilderItem> Steps = new();
    private ProgressListInstrument? Instrument;

    public FlowBuilderService(MacroProcessor processor) : base(processor)
    {
    }

    public void Add(string text, Action<FlowBuilderExecutionArguments> action)
    {
        Steps.Add(new()
        {
            Text = text,
            Execution = action
        });
    }

    public void Add(FlowBuilderItem item)
    {
        Steps.Add(item);
    }

    public void CreateInstrument()
    {
        var dashboard = Processor.GetService<DashboardService>();
        Instrument = dashboard.Create<ProgressListInstrument>(inst =>
        {
            inst.Title = Title ?? "Steps:";
        });

        foreach (var item in Steps)
        {
            Instrument.Add(new(ProgressStatus.Idle)
            {
                Text = item.IdleText ?? item.Text,
                Subtext = item.Subtext ?? SubtextType switch
                {
                    FlowBuilderSubtextType.Status => ProgressStatus.Idle.ToString(),
                    _ => null,
                },
            });
        }
    }

    private void UpdateItem(int index, ProgressInstrumentInfo info)
    {
        Instrument!.Update(index, new(info));
    }

    public void Execute()
    {
        if (Instrument is null)
        {
            CreateInstrument();
        }

        for (var i = 0; i < Steps.Count; i++)
        {
            var startTime = DateTime.Now;
            var startElapsedTime = Processor.GetElapsedTime();

            UpdateItem(i, new()
            {
                Status = ProgressStatus.Busy,
                Text = Steps[i].StartingText ?? Steps[i].Text,
                Subtext = Steps[i].Subtext ?? SubtextType switch
                {
                    FlowBuilderSubtextType.StartTime => startTime.ToString("HH:mm:ss"),
                    _ => null,
                },
            });

            var isSuccess = false;
            try
            {
                var args = new FlowBuilderExecutionArguments();
                args.Updated += info =>
                {
                    UpdateItem(i, info);
                };
                Steps[i].Execution.Invoke(args);
                isSuccess = true;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            var status = isSuccess ? ProgressStatus.Success : ProgressStatus.Failure;
            var duration = Processor.GetElapsedTime() - startElapsedTime;

            UpdateItem(i, new()
            {
                Status = status,
                Text = (isSuccess ? Steps[i].EndingText : Steps[i].ErrorText) ?? Steps[i].Text,
                Subtext = Steps[i].Subtext ?? SubtextType switch
                {
                    FlowBuilderSubtextType.EndTime => $"{DateTime.Now:HH:mm:ss}",
                    FlowBuilderSubtextType.Duration => $"{duration.TotalHours:X2}:{duration:mm:ss}",
                    FlowBuilderSubtextType.Status => $"{status}",
                    _ => null,
                },
            });

            if (Processor.IsCancelled)
            {
                break;
            }

            if (Interval > 0 && i < Steps.Count - 1)
            {
                Thread.Sleep(Interval);
            }

            if (!isSuccess)
            {
                break;
            }
        }
    }

}
