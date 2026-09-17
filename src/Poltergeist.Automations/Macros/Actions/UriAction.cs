using Poltergeist.Automations.Structures;

namespace Poltergeist.Automations.Macros;

public class UriAction : MacroAction
{
    public string? Uri { get; set; }

    public Func<ActionContext, string>? GetUri { get; set; }

    public UriAction()
    {
        Icon = IconInfo.FromGlyph("\uE71B");
        ActionIcon = IconInfo.FromGlyph("\uE8A7");
    }
}
