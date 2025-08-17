using System.Reflection;

namespace Poltergeist.Modules.Events;

public class AppEventChannel
{
    public string EventName { get; }

    public List<AppEventSubscription> Subscriptions = new();

    public bool IsStrictOneTime { get; }

    public bool HasFired { get; set; }

    public AppEventChannel(Type eventType)
    {
        EventName = AppEvent.GetEventName(eventType);
        IsStrictOneTime = eventType.GetCustomAttribute<StrictOneTimeAttribute>() is not null;
    }
}
