namespace Poltergeist.Automations.Components.Interactions;

public class TipModel : NotificationModel
{
    public string? Title { get; set; }
    public string? Text { get; set; }
    public int? Timeout { get; set; }
    public string? Glyph { get; set; }
    public TipType Type { get; set; }
}
