namespace Poltergeist.Automations.Macros;

public abstract class MacroAction
{
    public string? Key { get; init; }
    public string? Text { get; init; }
    public string? Description { get; init; }
    public string? Icon { get; set; }
}
