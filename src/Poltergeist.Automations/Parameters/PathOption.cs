using System.Collections.Generic;

namespace Poltergeist.Automations.Parameters;

public class PathOption : OptionItem<string>
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
