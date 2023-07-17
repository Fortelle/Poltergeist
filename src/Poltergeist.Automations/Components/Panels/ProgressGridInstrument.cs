using System.Collections.Generic;
using Poltergeist.Automations.Processors;
using Poltergeist.Common.Structures.Colors;

namespace Poltergeist.Automations.Components.Panels;

public class ProgressGridInstrument : GridInstrument<ProgressGridInstrumentItem>
{
    public static Dictionary<ProgressStatus, GridInstrumentItem> ProgressTemplates { get; set; } = new()
    {
        [ProgressStatus.Idle] = new() { Color = ThemeColor.Gray, Glyph = null },
        [ProgressStatus.Busy] = new() { Color = ThemeColor.Yellow, Glyph = "\uE9F5" },
        [ProgressStatus.Success] = new() { Color = ThemeColor.Green, Glyph = "\uE73E" },
        [ProgressStatus.Failure] = new() { Color = ThemeColor.Red, Glyph = "\uEDAE" },
        [ProgressStatus.Warning] = new() { Color = ThemeColor.Orange, Glyph = "\uEDB1" },
    };

    public ProgressGridInstrument(MacroProcessor processor) : base(processor)
    {
        foreach(var (key, value) in ProgressTemplates)
        {
            Templates.Add(key.ToString(), value);
        }
    }
}
