using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Automations.Components.Panels;

public class ProgressTileInstrument : TileInstrument<ProgressTileInstrumentItem>
{
    public static Dictionary<ProgressStatus, TileInstrumentItem> ProgressTemplates { get; set; } = new()
    {
        [ProgressStatus.Idle] = new() { Color = ThemeColor.Gray, Glyph = null },
        [ProgressStatus.Busy] = new() { Color = ThemeColor.Yellow, Glyph = "\uF16A" },
        [ProgressStatus.Success] = new() { Color = ThemeColor.Green, Glyph = "\uE73E" },
        [ProgressStatus.Failure] = new() { Color = ThemeColor.Red, Glyph = "\uEDAE" },
        [ProgressStatus.Warning] = new() { Color = ThemeColor.Orange, Glyph = "\uEDB1" },
    };

    public ProgressTileInstrument(MacroProcessor processor) : base(processor)
    {
        foreach(var (key, value) in ProgressTemplates)
        {
            Templates.Add(key.ToString(), value);
        }
    }
}
