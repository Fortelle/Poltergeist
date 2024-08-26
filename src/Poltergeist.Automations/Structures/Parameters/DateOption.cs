namespace Poltergeist.Automations.Structures.Parameters;

public class DateOption : OptionDefinition<DateOnly>
{
    public DateOption(string key) : base(key, default)
    {
    }

    public DateOption(string key, DateOnly defaultValue) : base(key, defaultValue)
    {
    }

}
