using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Poltergeist.Modules.Events;

public class AppEventService : ServiceBase
{
    private readonly Dictionary<Type, AppEventDelegator> Delegators = new();

    public void Subscribe<T>(Action<T> handler, bool once = false, int priority = 0) where T : AppEventHandler
    {
        var eventType = typeof(T);
        AddEventListener(new AppEventListener(eventType, handler)
        {
            Once = once,
            Priority = priority,
            Subscriber = GetCallingClassName(2),
            IsAsync = eventType.GetCustomAttribute<AsyncStateMachineAttribute>() is not null,
        });
    }

    public void Subscribe<T>(Func<T, Task> handler, bool once = false, int priority = 0) where T : AppEventHandler
    {
        AddEventListener(new AppEventListener(typeof(T), handler)
        {
            Once = once,
            Priority = priority,
            Subscriber = GetCallingClassName(2),
            IsAsync = true,
        });
    }

    public void Unsubscribe<T>(Action<T> handler) where T : AppEventHandler
    {
        var eventName = typeof(T).Name.Replace("Handler", null);

        if (!Delegators.TryGetValue(typeof(T), out var delegator))
        {
            Logger.Trace($"Failed to unsubscribe to event '{eventName}': Delegator not found.");
            return;
        }

        var listener = delegator.GetListener(handler);

        if (listener is null)
        {
            Logger.Trace($"Failed to unsubscribe to event '{eventName}': Listener not found.");
            return;
        }

        delegator.Listeners.Remove(listener);
        Logger.Trace($"Removed the listener from event '{eventName}'.");
    }

    public void Raise<T>(T handler, bool dispose = false) where T : AppEventHandler
    {
        InternalRaise(handler, dispose);
    }

    public T Raise<T>(bool dispose = false) where T : AppEventHandler, new()
    {
        var handler = new T();
        InternalRaise(handler, dispose);
        return handler;
    }

    public void SubscribeMethods(object target)
    {
        var subscriberType = typeof(AppEventSubscriberAttribute);
        var targetType = target.GetType();
        var targetMethods = targetType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var method in targetMethods)
        {
            if (method.GetCustomAttribute(subscriberType, true) is not AppEventSubscriberAttribute subscriberAttribute)
            {
                continue;
            }
            var methodParameters = method.GetParameters();
            var handlerType = methodParameters[0].ParameterType;
            var delegateType = Expression.GetDelegateType([.. methodParameters.Select(x => x.ParameterType), method.ReturnType]);
            var del = method.CreateDelegate(delegateType, target);
            var listener = new AppEventListener(handlerType, del)
            {
                Once = subscriberAttribute.Once,
                Priority = subscriberAttribute.Priority,
                Subscriber = targetType.Name
            };
            AddEventListener(listener);
        }
    }

    private void AddEventListener(AppEventListener listener)
    {
        if (!Delegators.TryGetValue(listener.Type, out var delegator))
        {
            delegator = new(listener.Type);
            Delegators.Add(listener.Type, delegator);
        }
        delegator.Listeners.Add(listener);

        Logger.Trace($"'{listener.Subscriber}' subscribed to event '{delegator.EventName}'.", new
        {
            delegator.EventName,
            listener.Subscriber,
            listener.Once,
            listener.Priority,
            listener.IsAsync,
        });
    }

    private void InternalRaise(AppEventHandler handler, bool dispose)
    {
        var eventType = handler.GetType();
        var eventName = eventType.Name.Replace("Handler", null);
        var publisher = GetCallingClassName(3);

        Logger.Trace($"Event '{eventName}' is emitted.", new { publisher });

        if (!Delegators.TryGetValue(eventType, out var delegator))
        {
            Logger.Trace($"Executed event '{eventName}': No listener is registered.");
            return;
        }

        var listeners = delegator.Listeners.OrderByDescending(x => x.Priority).ToArray();
        var index = 0;
        foreach (var listener in listeners)
        {
            Logger.Trace($"Executing the listener registered by '{listener.Subscriber}' for event '{eventName}'. ({index + 1}/{listeners.Length})", new
            {
                eventType.Name,
                publisher,
                listener.Subscriber,
                listener.IsAsync,
            });

            listener.Callback.DynamicInvoke(handler);

            if (listener.Once)
            {
                delegator.Listeners.Remove(listener);
                Logger.Trace($"Removed the listener from event '{eventName}'.", new
                {
                    Handler = eventType.Name,
                    Publisher = publisher,
                    Subscriber = listener.Subscriber,
                });
            }

            index += 1;
        }

        if (dispose && delegator.Listeners.Count > 0)
        {
            delegator.Listeners.Clear();
            Logger.Trace($"Removed all listeners from event '{eventName}'.", new
            {
                Handler = eventType.Name,
                Publisher = publisher,
            });
        }

        Logger.Trace($"Executed event '{eventName}' with {listeners.Length} listeners.");
    }

    private static string? GetCallingClassName(int skipFrames)
    {
#if DEBUG
        return new StackFrame(skipFrames, false).GetMethod()?.DeclaringType?.Name;
#else
        return null;
#endif
    }
}
