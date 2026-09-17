using Poltergeist.Automations.Structures;

namespace Poltergeist.Automations.Macros;

public class TerminalAction : MacroAction
{
    public required Func<TerminalActionContext, Task> ExecuteAsync { get; set; }

    public bool AutoExecute { get; set; }

    public TerminalAction()
    {
    }
}
