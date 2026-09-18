using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Automations.Components.Panels;

public class ProgressListInstrument : ListInstrument<ProgressListInstrumentItem>
{
    public static Dictionary<ProgressStatus, ListInstrumentItem> ProgressTemplates { get; set; } = new()
    {
        [ProgressStatus.Idle] = new() { Color = ThemeColor.Gray, Icon = new GlyphIcon("\uE9AE") },
        [ProgressStatus.Busy] = new() { Color = ThemeColor.Yellow, Icon = new GlyphIcon("\uF16A") },
        [ProgressStatus.Success] = new() { Color = ThemeColor.Green, Icon = new GlyphIcon("\uE930") },
        [ProgressStatus.Failure] = new() { Color = ThemeColor.Red, Icon = new GlyphIcon("\uEA39") },
        [ProgressStatus.Warning] = new() { Color = ThemeColor.Orange, Icon = new GlyphIcon("\uE7BA") },
    };

    public ProgressListInstrument(MacroProcessor processor) : base(processor)
    {
        foreach (var (key, value) in ProgressTemplates)
        {
            Templates.Add(key.ToString(), value);
        }
    }
}
