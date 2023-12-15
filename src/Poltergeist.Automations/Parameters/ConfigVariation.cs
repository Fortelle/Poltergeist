namespace Poltergeist.Automations.Parameters;

public class ConfigVariation
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Glyph { get; set; }

    public bool Normalized { get; set; }

    public Dictionary<string, object?>? Options { get; set; }

    public Dictionary<string, object?>? Environments { get; set; }

    public Dictionary<string, object?>? SessionStorage { get; set; }
}
