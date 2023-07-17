using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Macros;

public class BasicMacroExecutionArguments : ArgumentService
{
    public string? Message { get; set; }

    public BasicMacroExecutionArguments(MacroProcessor processor) : base(processor)
    {
    }

}
