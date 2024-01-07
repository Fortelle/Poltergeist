namespace Poltergeist.Services;

public class MacroSummaryEntry
{
    public required string MacroKey { get; set; }
    public required string Title { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime LastRunTime { get; set; }
    public int RunCount { get; set; }
}
