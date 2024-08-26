namespace Poltergeist.Automations.Structures.Parameters;

public class PathOption : OptionDefinition<string>
{
    public PathOptionMode Mode { get; set; }

    public Dictionary<string, IList<string>>? Filters { get; set; }

    public PathOption(string key) : base(key)
    {
    }

    public PathOption(string key, string defaultValue) : base(key, defaultValue)
    {
    }
}
