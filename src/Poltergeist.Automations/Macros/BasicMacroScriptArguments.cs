using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Macros;

public class BasicMacroScriptArguments : ArgumentService
{
    public string Message { get; set; }

    public BasicMacroScriptArguments(MacroProcessor processor) : base(processor)
    {
    }

}
