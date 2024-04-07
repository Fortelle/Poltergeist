namespace Poltergeist.Automations.Parameters;

public class ConfigVariation
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Icon { get; set; }

    public bool IgnoresUserOptions { get; set; }

    public bool IsDevelopmentOnly { get; set; }

    public Dictionary<string, object?>? OptionOverrides { get; set; }

    public Dictionary<string, object?>? EnvironmentOverrides { get; set; }

    public Dictionary<string, object?>? SessionStorage { get; set; }
}
