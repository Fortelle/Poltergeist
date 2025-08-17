namespace Poltergeist.Modules.Events;

public abstract class AppEvent
{
    private const string EventNameSuffix = "Event";

    public static string GetEventName(string name)
    {
        if (name.EndsWith(EventNameSuffix))
        {
            return name[..^EventNameSuffix.Length];
        }
        else
        {
            return name;
        }
    }

    public static string GetEventName(Type type)
    {
        return GetEventName(type.Name);
    }
}
