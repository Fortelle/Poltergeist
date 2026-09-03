using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Macros.Oneshots;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Examples.Macros;

[ExampleMacro]
public class ListInstrumentExample : CommonOneshotMacroBase
{
    public ListInstrumentExample() : base()
    {
        Title = "ListInstrument";

        Category = "Instruments";

        Description = "This example shows how to use the ListInstrument.";
    }

    protected override void OnExecute(WorkflowController controller)
    {
        var dashboard = controller.Processor.GetService<DashboardService>();

        CreateBasicInstrument(dashboard, 3, TimeSpan.FromSeconds(1));
        CreateTemplatedInstrument(dashboard, 3, TimeSpan.FromSeconds(1));
        CreateProgressListInstrument(dashboard, 3, TimeSpan.FromSeconds(1));
        CreateProgressBar(dashboard, 3, TimeSpan.FromSeconds(1));
    }

    private static void CreateBasicInstrument(DashboardService dashboard, int count, TimeSpan duration)
    {
        var instrument = dashboard.Create<ListInstrument>(gi =>
        {
            gi.Title = "Basic:";
        });

        for (var i = 0; i < count; i++)
        {
            instrument.Add(new()
            {
                Color = ThemeColor.Gray,
                Icon = IconInfo.FromGlyph("\uE9AE"),
                Text = $"Step {i + 1}"
            });
        }

        for (var i = 0; i < count; i++)
        {
            instrument.Update(i, new()
            {
                Color = ThemeColor.Yellow,
                Icon = IconInfo.FromGlyph("\uF16A")
            });
            Thread.Sleep(duration);
            instrument.Update(i, new()
            {
                Color = ThemeColor.Green,
                Icon = IconInfo.FromGlyph("\uE930")
            });
        }
    }

    private static void CreateTemplatedInstrument(DashboardService dashboard, int count, TimeSpan duration)
    {
        var instrument = dashboard.Create<ListInstrument>(gi =>
        {
            gi.Title = "Using templates:";
            gi.Templates.Add("idle", new() { Color = ThemeColor.Gray, Icon = IconInfo.FromGlyph("\uE9AE") });
            gi.Templates.Add("busy", new() { Color = ThemeColor.Yellow, Icon = IconInfo.FromGlyph("\uF16A") });
            gi.Templates.Add("success", new() { Color = ThemeColor.Green, Icon = IconInfo.FromGlyph("\uE930") });
            gi.Templates.Add("failure", new() { Color = ThemeColor.Red, Icon = IconInfo.FromGlyph("\uEA39") });
        });

        for (var i = 0; i < count; i++)
        {
            instrument.Add(new()
            {
                TemplateKey = "idle",
                Text = $"Step {i + 1}",
            });
        }

        for (var i = 0; i < count; i++)
        {
            instrument.Update(i, new()
            {
                TemplateKey = "busy"
            });
            Thread.Sleep(duration);
            instrument.Update(i, new()
            {
                TemplateKey = "success"
            });
        }
    }

    private static void CreateProgressListInstrument(DashboardService dashboard, int count, TimeSpan duration)
    {
        var instrument = dashboard.Create<ProgressListInstrument>(gi =>
        {
            gi.Title = "Using ProgressListInstrument:";
        });

        for (var i = 0; i < count; i++)
        {
            instrument.Add(new(ProgressStatus.Idle)
            {
                Text = $"Step {i + 1}",
            });
        }

        for (var i = 0; i < count; i++)
        {
            instrument.Update(i, new(ProgressStatus.Busy));
            Thread.Sleep(duration);
            instrument.Update(i, new(ProgressStatus.Success));
        }

    }

    private static void CreateProgressBar(DashboardService dashboard, int count, TimeSpan duration)
    {
        var instrument = dashboard.Create<ProgressListInstrument>(gi =>
        {
            gi.Title = "ProgressBar:";
        });

        for (var i = 0; i < count; i++)
        {
            instrument.Add(new(ProgressStatus.Idle)
            {
                Text = $"Step {i + 1}",
            });
        }

        for (var i = 0; i < count; i++)
        {
            for (var j = 0; j < 10; j++)
            {
                var progress = (j + 1) / 10d;
                instrument.Update(i, new(ProgressStatus.Busy)
                {
                    Progress = progress,
                    Subtext = $"{progress:P0}"
                });
                Thread.Sleep((int)(duration.TotalMilliseconds / 5d));
            }

            instrument.Update(i, new(ProgressStatus.Success));
        }
    }
}
