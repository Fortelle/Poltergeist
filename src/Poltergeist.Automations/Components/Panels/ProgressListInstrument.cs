using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Automations.Components.Panels;

public class ProgressListInstrument : ListInstrument<ProgressListInstrumentItem>
{
    public static Dictionary<ProgressStatus, ListInstrumentItem> ProgressTemplates { get; set; } = new()
    {
        [ProgressStatus.Idle] = new() { Color = ThemeColor.Gray, Icon = IconInfo.FromGlyph("\uE9AE") },
        [ProgressStatus.Busy] = new() { Color = ThemeColor.Yellow, Icon = IconInfo.FromGlyph("\uF16A") },
        [ProgressStatus.Success] = new() { Color = ThemeColor.Green, Icon = IconInfo.FromGlyph("\uE930") },
        [ProgressStatus.Failure] = new() { Color = ThemeColor.Red, Icon = IconInfo.FromGlyph("\uEA39") },
        [ProgressStatus.Warning] = new() { Color = ThemeColor.Orange, Icon = IconInfo.FromGlyph("\uE7BA") },
    };

    public ProgressListInstrument(MacroProcessor processor) : base(processor)
    {
        foreach (var (key, value) in ProgressTemplates)
        {
            Templates.Add(key.ToString(), value);
        }
    }
}
