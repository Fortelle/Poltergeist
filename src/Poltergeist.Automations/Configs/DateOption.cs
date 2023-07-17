using System;

namespace Poltergeist.Automations.Configs;

public class DateOption : OptionItem<DateOnly>
{
    public DateOption(string key) : base(key, default)
    {
    }

    public DateOption(string key, DateOnly defaultValue) : base(key, defaultValue)
    {
    }

}
