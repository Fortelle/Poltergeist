using System.Reflection;
using System.Runtime.CompilerServices;

namespace Poltergeist.Modules.Events;

public class AppEventSubscription
{
    public Type EventType { get; init; }

    public Delegate Callback { get; init; }

    public AppEventSubscriptionOptions Options { get; init; }

    public string? Subscriber { get; init; }

    public bool IsAsync { get; }

    public string EventName { get; }

    public AppEventSubscription(Type eventType, Delegate callback, AppEventSubscriptionOptions options)
    {
        EventType = eventType;
        Callback = callback;
        Options = options;
        IsAsync = eventType.GetCustomAttribute<AsyncStateMachineAttribute>() is not null;
        EventName = AppEvent.GetEventName(eventType);
    }
}
