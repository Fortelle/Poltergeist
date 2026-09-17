using System.Diagnostics;
using Poltergeist.Automations.Structures;

namespace Poltergeist.Automations.Macros;

public class ExternalProcessAction : MacroAction
{
    public required Func<ActionContext, ProcessStartInfo> GetStartInfo { get; set; }

    public ExternalProcessAction()
    {
        Icon = IconInfo.FromGlyph("\uE756");
        ActionIcon = IconInfo.FromGlyph("\uE8A7");
    }
}
