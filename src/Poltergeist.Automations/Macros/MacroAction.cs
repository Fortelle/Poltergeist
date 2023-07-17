using System;

namespace Poltergeist.Automations.Macros;

public class MacroAction
{
    public required string Text { get; set; }
    public required Action<MacroActionArguments> Execute;
    public string? Glyph { get; set; }
}
