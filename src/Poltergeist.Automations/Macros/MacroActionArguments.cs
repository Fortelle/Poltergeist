using System.Collections.Generic;

namespace Poltergeist.Automations.Macros;

public class MacroActionArguments
{
    public IMacroBase Macro { get; init; }

    public required Dictionary<string, object?> Options { get; init; }

    public required Dictionary<string, object?> Environments { get; init; }

    //public nint Hwnd { get; set; }

    public string? Message { get; set; }

    public MacroActionArguments(IMacroBase macro)
    {
        Macro = macro;
    }

}
