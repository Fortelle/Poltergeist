using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Poltergeist.Modules.Events;

public class AppEventService : ServiceBase
{
    private readonly Dictionary<Type, AppEventChannel> Channels = new();

    public AppEventService()
    {
    }

    public void Subscribe<T>(Action<T> callback, AppEventSubscriptionOptions? options = null) where T : AppEvent
    {
        AddSubscription(new AppEventSubscription(typeof(T), callback, options ?? new())
        {
            Subscriber = GetCallingClassName(2),
        });
    }

    public void Subscribe(Type type, Delegate callback, AppEventSubscriptionOptions? options = null)
    {
        AddSubscription(new (type, callback, options ?? new())
        {
            Subscriber = GetCallingClassName(2),
        });
    }

    private void AddSubscription(AppEventSubscription subscription)
    {
        var channel = GetOrCreateEventChannel(subscription.EventType);

        if (channel.IsStrictOneTime && channel.HasFired)
        {
            throw new InvalidOperationException($"Cannot subscribe to one-time event {channel.EventName} after it has been fired.");
        }

        channel.Subscriptions.Add(subscription);

        Logger.Trace($"'{subscription.Subscriber}' subscribed to event '{subscription.EventName}'.", new
        {
            subscription.EventName,
            subscription.Subscriber,
            subscription.Options,
        });
    }

    private void ExecuteSubscription(AppEventSubscription subscription, AppEvent? @event)
    {
        subscription.Callback.DynamicInvoke(@event);
    }

    public bool Unsubscribe<T>(Action<T> handler) where T : AppEvent
    {
        var eventName = AppEvent.GetEventName(typeof(T));

        if (!Channels.TryGetValue(typeof(T), out var channel))
        {
            Logger.Trace($"Failed to unsubscribe to event '{eventName}': The handler is not registered.");
            return false;
        }

        var subscription = channel.Subscriptions.FirstOrDefault(x => x.Callback.Equals(handler));

        if (subscription is null)
        {
            Logger.Trace($"Failed to unsubscribe to event '{eventName}': The handler is not registered.");
            return false;
        }

        channel.Subscriptions.Remove(subscription);

        Logger.Trace($"Removed the subscription from event '{eventName}'.");

        return true;
    }

    public void Publish<T>(T handler) where T : AppEvent
    {
        PublishInternal(handler);
    }

    public T Publish<T>() where T : AppEvent, new()
    {
        var handler = new T();
        PublishInternal(handler);
        return handler;
    }

    public void SubscribeMethods(object target)
    {
        var targetType = target.GetType();
        var targetMethods = targetType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var method in targetMethods)
        {
            var subscriberAttribute = method.GetCustomAttribute<AppEventSubscriberAttribute>(inherit: true);
            if (subscriberAttribute is null)
            {
                continue;
            }
            var options = new AppEventSubscriptionOptions()
            {
                Once = subscriberAttribute.Once,
                Priority = subscriberAttribute.Priority,
            };
            var methodParameters = method.GetParameters();
            var eventType = methodParameters[0].ParameterType;
            var handlerType = Expression.GetDelegateType([.. methodParameters.Select(x => x.ParameterType), method.ReturnType]);
            var handler = method.CreateDelegate(handlerType, target);
            AddSubscription(new(eventType, handler, options)
            {
                Subscriber = targetType.Name,
            });
        }
    }

    private AppEventChannel GetOrCreateEventChannel(Type eventType)
    {
        if (!Channels.TryGetValue(eventType, out var channel))
        {
            channel = new AppEventChannel(eventType);
            Channels.Add(eventType, channel);
            Logger.Trace($"Created event channel '{channel.EventName}'.");
        }

        return channel;
    }

    private void PublishInternal(AppEvent @event)
    {
        var eventType = @event.GetType();
        var eventName = AppEvent.GetEventName(eventType);
        var publisher = GetCallingClassName(3);

        Logger.Trace($"Event '{eventName}' is emitted.", new { publisher });

        var channel = GetOrCreateEventChannel(eventType);

        if (channel.IsStrictOneTime && channel.HasFired)
        {
            throw new InvalidOperationException("Cannot publish a one-time event after it has been fired.");
        }

        var subscriptions = channel.Subscriptions.OrderByDescending(x => x.Options.Priority).ToArray();
        var index = 0;
        foreach (var subscription in subscriptions)
        {
            Logger.Trace($"Executing the subscription registered by '{subscription.Subscriber}' for event '{eventName}'. ({index + 1}/{subscriptions.Length})", new
            {
                eventName,
                publisher,
                subscription.Subscriber,
            });

            ExecuteSubscription(subscription, @event);

            if (channel.IsStrictOneTime)
            {
                channel.Subscriptions.Remove(subscription);
                Logger.Trace($"Removed the subscription from one-time event '{eventName}'.", new
                {
                    eventName,
                    publisher,
                    subscription.Subscriber,
                });
            }
            else if (subscription.Options.Once)
            {
                channel.Subscriptions.Remove(subscription);
                Logger.Trace($"Removed the one-time subscription from event '{eventName}'.", new
                {
                    eventName,
                    publisher,
                    subscription.Subscriber,
                });
            }

            index += 1;
        }

        if (channel.IsStrictOneTime)
        {
            channel.HasFired = true;
        }

        Logger.Trace($"Executed event '{eventName}' with {subscriptions.Length} subscriptions.");
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
