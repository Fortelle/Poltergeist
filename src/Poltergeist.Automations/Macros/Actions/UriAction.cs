using Poltergeist.Automations.Structures;

namespace Poltergeist.Automations.Macros;

public class UriAction : MacroAction
{
    public string? Uri { get; set; }

    public Func<ActionContext, string>? GetUri { get; set; }

    public UriAction()
    {
        Icon = new GlyphIcon("\uE71B");
        ActionIcon = new GlyphIcon("\uE8A7");
    }
}
