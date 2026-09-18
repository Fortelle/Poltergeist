using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Structures;
using Poltergeist.Automations.Structures.Colors;

namespace Poltergeist.Automations.Components.Panels;

public class ProgressTileInstrument : TileInstrument<ProgressTileInstrumentItem>
{
    public static Dictionary<ProgressStatus, TileInstrumentItem> ProgressTemplates { get; set; } = new()
    {
        [ProgressStatus.Idle] = new()
        {
            Color = ThemeColor.Gray
        },
        [ProgressStatus.Busy] = new()
        {
            Color = ThemeColor.Yellow,
            Icon = new GlyphIcon("\uF16A")
            {
                Animation = new SpinAnimation(),
            }
        },
        [ProgressStatus.Success] = new()
        {
            Color = ThemeColor.Green,
            Icon = new GlyphIcon("\uE73E")
        },
        [ProgressStatus.Failure] = new()
        {
            Color = ThemeColor.Red,
            Icon = new GlyphIcon("\uEDAE")
        },
        [ProgressStatus.Warning] = new()
        {
            Color = ThemeColor.Orange,
            Icon = new GlyphIcon("\uEDB1")
        },
    };

    public ProgressTileInstrument(MacroProcessor processor) : base(processor)
    {
        foreach(var (key, value) in ProgressTemplates)
        {
            Templates.Add(key.ToString(), value);
        }
    }
}
