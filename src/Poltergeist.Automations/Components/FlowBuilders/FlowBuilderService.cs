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

    public FlowBuilderService(MacroProcessor proc) : base(proc)
    {
    }

    public void Add(string text, Action<FlowBuilderArguments> action)
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
        var instruments = Processor.GetService<DashboardService>();
        Instrument = instruments.Create<ProgressListInstrument>(inst =>
        {
            inst.Title = Title ?? "Steps:";
        });

        foreach (var item in Steps)
        {
            Instrument.Add(new(ProgressStatus.Idle)
            {
                Text = item.IdleText ?? item.Text,
                Subtext = SubtextType switch
                {
                    FlowBuilderSubtextType.Status => ProgressStatus.Idle.ToString(),
                    _ => item.Subtext,
                },
            });
        }
    }

    public void Execute()
    {
        if (Instrument is null)
        {
            CreateInstrument();
        }

        for (var i = 0; i < Steps.Count; i++)
        {
            double? max = null;
            double? current = null;
            var status = ProgressStatus.Idle;
            var text = Steps[i].StartingText ?? Steps[i].Text;
            var subtext = Steps[i].Subtext;
            var startTime = default(DateTime);
            var endTime = default(DateTime);

            void resetSubtext()
            {
                subtext = Steps[i].Subtext;
            }

            void updateInstrumentItem()
            {
                var progress = 1d;
                if (max.HasValue && current.HasValue)
                {
                    progress = current.Value / max.Value;
                    subtext ??= max > 1 ? $"{current:#}/{max:#}" : $"{progress:#%}";
                }
                else if (current.HasValue)
                {
                    subtext ??= $"{current}";
                }

                subtext ??= SubtextType switch
                {
                    FlowBuilderSubtextType.StartTime when startTime != default => startTime.ToString("HH:mm:ss"),
                    FlowBuilderSubtextType.EndTime when endTime != default => endTime.ToString("HH:mm:ss"),
                    FlowBuilderSubtextType.Duration when endTime != default => (endTime - startTime).ToString("hh\\:mm\\:ss"),
                    FlowBuilderSubtextType.Status => status.ToString(),
                    FlowBuilderSubtextType.None => "",
                    _ => null,
                };

                Instrument!.Update(i, new(status)
                {
                    Text = text,
                    Subtext = subtext,
                    Progress = progress,
                });
                resetSubtext();
            }

            status = ProgressStatus.Busy;
            updateInstrumentItem();

            var args = new FlowBuilderArguments();
            args.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(FlowBuilderArguments.Text):
                        text = args.Text;
                        break;
                    case nameof(FlowBuilderArguments.Subtext):
                        subtext = args.Subtext;
                        break;
                    case nameof(FlowBuilderArguments.Max):
                        max = args.Max;
                        break;
                    case nameof(FlowBuilderArguments.Current):
                        current = args.Current;
                        break;
                }
                updateInstrumentItem();
            };

            startTime = DateTime.Now;
            try
            {
                Steps[i].Execution.Invoke(args);
                status = ProgressStatus.Success;
                text = Steps[i].EndingText ?? Steps[i].Text;
            }
            catch (Exception e)
            {
                status = ProgressStatus.Failure;
                text = Steps[i].ErrorText ?? Steps[i].Text;
                Logger.Error(e.Message);

                if(Processor.IsCancelled)
                {
                    throw;
                }
            }
            endTime = DateTime.Now;

            updateInstrumentItem();

            if (Interval > 0 && i < Steps.Count - 1)
            {
                Thread.Sleep(Interval);
            }

            if (status != ProgressStatus.Success)
            {
                break;
            }

        }
    }

}
