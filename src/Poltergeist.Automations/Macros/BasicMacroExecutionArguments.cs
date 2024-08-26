using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Macros;

public class BasicMacroExecutionArguments(MacroProcessor processor) : ArgumentService(processor)
{
    public string? Message { get; set; }
}
