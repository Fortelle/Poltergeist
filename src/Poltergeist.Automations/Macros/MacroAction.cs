namespace Poltergeist.Automations.Macros;

public class MacroAction
{
    public string? Key { get; init; }
    public required string Text { get; init; }
    public string? Description { get; init; }
    public Action<MacroActionArguments>? Execute { get; set; }
    public Func<MacroActionArguments, Task>? ExecuteAsync { get; set; }
    public string? Glyph { get; set; }

    public string? ProgressTitle { get; set; }
    public bool IsCancellable { get; set; }
}
