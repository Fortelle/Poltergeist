namespace Poltergeist.Modules.Macros;

public class MacroInstanceProperties
{
    public string? Key { get; set; }
    public string? Title { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime? CreatedTime { get; set; }
    public DateTime? LastRunTime { get; set; }
    public int RunCount { get; set; }
}
