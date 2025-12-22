using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Automations.Components.Panels;

public class ProgressTileInstrument : TileInstrument<ProgressTileInstrumentItem>
{
    public static Dictionary<ProgressStatus, TileInstrumentItem> ProgressTemplates { get; set; } = new()
    {
        [ProgressStatus.Idle] = new() { Color = ThemeColor.Gray },
        [ProgressStatus.Busy] = new() { Color = ThemeColor.Yellow, Icon = IconInfo.FromGlyph("\uF16A") },
        [ProgressStatus.Success] = new() { Color = ThemeColor.Green, Icon = IconInfo.FromGlyph("\uE73E") },
        [ProgressStatus.Failure] = new() { Color = ThemeColor.Red, Icon = IconInfo.FromGlyph("\uEDAE") },
        [ProgressStatus.Warning] = new() { Color = ThemeColor.Orange, Icon = IconInfo.FromGlyph("\uEDB1") },
    };

    public ProgressTileInstrument(MacroProcessor processor) : base(processor)
    {
        foreach(var (key, value) in ProgressTemplates)
        {
            Templates.Add(key.ToString(), value);
        }
    }
}
