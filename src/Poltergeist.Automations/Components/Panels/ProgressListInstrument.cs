using System.Collections.Generic;
using Poltergeist.Automations.Processors;
using Poltergeist.Common.Structures.Colors;

namespace Poltergeist.Automations.Components.Panels;

public class ProgressListInstrument : ListInstrument<ProgressListInstrumentItem>
{
    public static Dictionary<ProgressStatus, ListInstrumentItem> ProgressTemplates { get; set; } = new()
    {
        [ProgressStatus.Idle] = new() { Color = ThemeColor.Gray, Glyph = "\uE9AE" },
        [ProgressStatus.Busy] = new() { Color = ThemeColor.Yellow, Glyph = "\uF16A" },
        [ProgressStatus.Success] = new() { Color = ThemeColor.Green, Glyph = "\uE930" },
        [ProgressStatus.Failure] = new() { Color = ThemeColor.Red, Glyph = "\uEA39" },
        [ProgressStatus.Warning] = new() { Color = ThemeColor.Orange, Glyph = "\uE7BA" },
    };

    public ProgressListInstrument(MacroProcessor processor) : base(processor)
    {
        foreach (var (key, value) in ProgressTemplates)
        {
            Templates.Add(key.ToString(), value);
        }
    }
}
