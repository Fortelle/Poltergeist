namespace Poltergeist.Automations.Macros;

public class MacroProperties
{
    public required string TemplateKey { get; init; }
    public required string ShellKey { get; init; }

    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime? CreatedTime { get; set; }
    public DateTime? LastRunTime { get; set; }
    public int? RunCount { get; set; }
}
