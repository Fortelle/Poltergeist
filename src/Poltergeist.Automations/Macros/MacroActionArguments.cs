using Poltergeist.Automations.Components.Interactions;

namespace Poltergeist.Automations.Macros;

public class MacroActionArguments
{
    public IMacroBase Macro { get; init; }

    public required Dictionary<string, object?> Options { get; init; }

    public required Dictionary<string, object?> Environments { get; init; }

    public CancellationToken CancellationToken { get; set; }

    public string? Message { get; set; }

    public MacroActionArguments(IMacroBase macro)
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
