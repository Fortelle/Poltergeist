using Poltergeist.Automations.Structures;

namespace Poltergeist.Automations.Macros;

public abstract class MacroAction
{
    public string? Key { get; init; }
    public string? Text { get; set; }
    public string? Description { get; set; }
    public IconInfo? Icon { get; set; }
    public IconInfo? ActionIcon { get; set; }
}
