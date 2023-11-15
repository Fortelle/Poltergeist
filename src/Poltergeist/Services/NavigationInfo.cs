namespace Poltergeist.Services;

public class NavigationInfo
{
    public required string Key { get; set; }
    public string? Header { get; set; }
    public string? Glyph { get; set; }
    public bool IsFooter { get; set; }

    public Func<string[], object?, object?>? CreateContent { get; set; }
    public Func<object, object>? CreateHeader { get; set; }
};
