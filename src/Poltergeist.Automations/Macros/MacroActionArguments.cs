using Poltergeist.Automations.Components.Interactions;

namespace Poltergeist.Automations.Macros;

public class MacroActionArguments
{
    public IUserMacro Macro { get; init; }

    public required IReadOnlyDictionary<string, object?> Options { get; init; }

    public required IReadOnlyDictionary<string, object?> Environments { get; init; }

    public CancellationToken CancellationToken { get; set; }

    public string? Message { get; set; }

    public MacroActionArguments(IUserMacro macro)
    {
        Macro = macro;
    }

    public void ShowTooltip(string message)
    {
        _ = InteractionService.UIShowAsync(new TipModel()
        {
            Text = message,
        });
    }
}
