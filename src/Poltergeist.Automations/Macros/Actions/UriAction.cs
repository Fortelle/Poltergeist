namespace Poltergeist.Automations.Macros;

public class UriAction : MacroAction
{
    public string? Uri { get; set; }

    public Func<ActionContext, string>? GetUri { get; set; }
}
