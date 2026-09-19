using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Operations.Inputting;

public class ParseNamedPositionHook : MacroHook
{
    public NamedPosition Input { get; }

    public PositionToken? Output { get; set; }

    public ParseNamedPositionHook(NamedPosition input)
    {
        Input = input;
    }
}
