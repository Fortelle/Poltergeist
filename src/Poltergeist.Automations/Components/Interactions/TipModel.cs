namespace Poltergeist.Automations.Components.Interactions;

public class TipModel : InteractionModel
{
    public string? Title { get; set; }
    public string? Text { get; set; }
    public int? Timeout { get; set; }
    public string? Glyph { get; set; }
    public TipType Type { get; set; }
}
