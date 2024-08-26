using System.Collections.Generic;
using Poltergeist.Automations.Components.Panels;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Automations.Components.Logging;

public class OutputService : MacroService
{
    private readonly Dictionary<OutputLevel, ListInstrumentItem> Templates = new()
    {
        [OutputLevel.None] = new() { Color = ThemeColor.Gray },
        [OutputLevel.Information] = new() { Color = ThemeColor.Azure, Glyph = "\uE946" },
        [OutputLevel.Success] = new() { Color = ThemeColor.Green, Glyph = "\uE930" },
        [OutputLevel.Failure] = new() { Color = ThemeColor.Red, Glyph = "\uEA39" },
        [OutputLevel.Attention] = new() { Color = ThemeColor.Orange, Glyph = "\uE7BA" },
    };

    private ListInstrument? OutputInstrument;

    public OutputService(MacroProcessor processor) : base(processor)
    {
    }

    public void Initialize()
    {
        if (OutputInstrument is not null)
        {
            return;
        }

        OutputInstrument = Processor.GetService<DashboardService>().Create<ListInstrument>(li =>
        {
            li.Title = "Output:";
            foreach (var (key, value) in Templates)
            {
                li.Templates.Add(key.ToString(), value);
            }
        });
    }

    public void Write(OutputLevel level, string text, string? subtext = null)
    {
        if (OutputInstrument is null)
        {
            Initialize();
        }

        OutputInstrument!.Add(new()
        {
            Text = text,
            Subtext = subtext,
            TemplateKey = level.ToString(),
        });
    }

    public void Write(string text, string? subtext = null)
    {
        Write(OutputLevel.Information, text, subtext);
    }

    public void NewGroup(string title)
    {
        OutputInstrument = Processor.GetService<DashboardService>().Create<ListInstrument>(li =>
        {
            li.Title = title;
            foreach (var (key, value) in Templates)
            {
                li.Templates.Add(key.ToString(), value);
            }
        });
    }


}
