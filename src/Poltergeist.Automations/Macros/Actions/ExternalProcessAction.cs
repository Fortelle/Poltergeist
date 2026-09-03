using System.Diagnostics;

namespace Poltergeist.Automations.Macros;

public class ExternalProcessAction : MacroAction
{
    public required Func<ActionContext, ProcessStartInfo> GetStartInfo { get; set; }
}
