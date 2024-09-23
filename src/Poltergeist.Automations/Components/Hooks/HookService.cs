using System.Diagnostics;
using Poltergeist.Automations.Components.Logging;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Components.Hooks;

public sealed class HookService : KernelService
{
    private readonly Dictionary<Type, HookDelegator> Delegators = new();

    private readonly LoggerWrapper Logger;

    public HookService(MacroProcessor processor, MacroLogger loggerService) : base(processor)
    {
        Logger = new(loggerService, nameof(HookService));

        Logger.Trace($"Kernel service '{nameof(HookService)}' is activated.");
    }

    public void Register<T>(Action<T> handler, bool once = false, int priority = 0) where T : MacroHook
    {
        InternalRegister(new HookListener(typeof(T), handler)
        {
            Priority = priority,
            Once = once,
            Subscriber = GetCallingClassName(2),
        });
    }

    public void Raise<T>(T hook, bool dispose = false) where T : MacroHook
    {
        InternalRaise(hook, dispose);
    }

    public void Raise<T>(bool dispose = false) where T : MacroHook, new()
    {
        InternalRaise(new T(), dispose);
    }

    public void RaiseUntil<T>(T hook, Func<T, bool> predicate) where T : MacroHook
    {
        InternalRaise(hook, false, predicate);
    }

    public void Unregister<T>(Action<T> handler) where T : MacroHook
    {
        InternalUnregister(typeof(T), handler);
    }

    private void InternalRegister(HookListener listener)
    {
        if (!Delegators.TryGetValue(listener.Type, out var delegator))
        {
            delegator = new(listener.Type);
            Delegators.Add(listener.Type, delegator);
        }

        delegator.Listeners.Add(listener);

        Logger.Trace($"A listener is attatched to hook '{delegator.Name}'.", new
        {
            delegator.Name,
            listener.Subscriber,
            listener.Once,
            listener.Priority,
        });
    }

    private void InternalRaise<T>(T hook, bool dispose, Func<T, bool>? untilPredicate = null) where T : MacroHook
    {
        var type = typeof(T);

        Logger.Trace($"Hook '{type.Name}' is triggered.");
        Logger.IncreaseIndent();

        if (!Delegators.TryGetValue(type, out var delegator))
        {
            Logger.Trace($"No listener is attatched to hook '{type.Name}'.");
            Logger.DecreaseIndent();
            return;
        }

        hook.Processor = Processor;

        var listeners = delegator.Listeners.OrderByDescending(x => x.Priority).ToArray();
        var index = 0;

        foreach (var listener in listeners)
        {
            Logger.Trace($"Executing the listener registered by '{listener.Subscriber}' for hook '{delegator.Name}' ({index + 1}/{listeners.Length}).");

            listener.Callback.DynamicInvoke(hook);

            if (listener.Once)
            {
                delegator.Listeners.Remove(listener);
                Logger.Trace($"Removed the listener from hook '{delegator.Name}'.");
            }

            if (untilPredicate?.Invoke(hook) == true)
            {
                break;
            }

            index += 1;
        }

        if (dispose && delegator.Listeners.Count > 0)
        {
            delegator.Listeners.Clear();
            Logger.Trace($"Removed all listeners from hook '{delegator.Name}'.");
        }

        Logger.Trace($"Hook '{type.Name}' is performed.");
        Logger.DecreaseIndent();
    }

    private void InternalUnregister(Type type, Delegate del)
    {
        if (!Delegators.TryGetValue(type, out var delegator))
        {
            return;
        }

        var listener = delegator.Listeners.FirstOrDefault(x => x.Callback == del);

        if (listener is null)
        {
            return;
        }

        delegator.Listeners.Remove(listener);
        Logger.Trace($"Removed the listener from hook '{type.Name}'.");
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
