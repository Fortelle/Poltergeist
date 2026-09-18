using System.Diagnostics;
using Poltergeist.Automations.Structures;

namespace Poltergeist.Automations.Macros;

public class ExternalProcessAction : MacroAction
{
    public required Func<ActionContext, ProcessStartInfo> GetStartInfo { get; set; }

    public ExternalProcessAction()
    {
        Icon = new GlyphIcon("\uE756");
        ActionIcon = new GlyphIcon("\uE8A7");
    }
}
