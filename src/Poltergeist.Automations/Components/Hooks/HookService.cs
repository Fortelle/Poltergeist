using System.Diagnostics;
using System.Reflection;
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

        Logger.Debug($"Kernel service {nameof(HookService)} is instantiated.");
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

    public void Register(HookListener listener)
    {
        InternalRegister(listener);
    }

    public void Raise<T>(T hook) where T : MacroHook
    {
        InternalRaise(hook);
    }

    public void Raise<T>() where T : MacroHook, new()
    {
        InternalRaise(new T());
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

        LoggerTrace(listener.Type, $"A listener is registered to hook '{delegator.Type.Name}'.", new { Hook = delegator.Type.Name, listener.Subscriber });
    }

    private void InternalRaise<T>(T hook) where T : MacroHook
    {
        var hookType = typeof(T);

        Delegators.TryGetValue(hookType, out var delegator);

        LoggerTrace(hookType, $"Hook '{hookType.Name}' is triggered.", new { Listeners = delegator?.Listeners.Count ?? 0 });

        if (delegator is null || delegator.Listeners.Count == 0)
        {
            return;
        }

        hook.Processor = Processor;

        var listeners = delegator.Listeners.OrderByDescending(x => x.Priority).ToArray();

        Logger.IncreaseIndent();

        foreach (var listener in listeners)
        {
            LoggerTrace(hookType, $"Executing the callback.", new { Hook = delegator.Type.Name, listener.Subscriber, listener.Once });
            Logger.IncreaseIndent();

            listener.Callback.DynamicInvoke(hook);

            if (listener.Once)
            {
                delegator.Listeners.Remove(listener);
            }

            Logger.DecreaseIndent();
        }

        Logger.DecreaseIndent();
    }

    private void InternalUnregister(Type hookType, Delegate del)
    {
        if (!Delegators.TryGetValue(hookType, out var delegator))
        {
            return;
        }

        var listener = delegator.Listeners.FirstOrDefault(x => x.Callback == del);

        if (listener is null)
        {
            return;
        }

        delegator.Listeners.Remove(listener);

        LoggerTrace(hookType, $"A listener is removed from hook '{delegator.Type.Name}'.", new { Hook = delegator.Type.Name, listener.Subscriber });
    }

    private static string? GetCallingClassName(int skipFrames)
    {
#if DEBUG
        return new StackFrame(skipFrames, false).GetMethod()?.DeclaringType?.Name;
#else
        return null;
#endif
    }

    #region "Logging"

    private readonly Dictionary<Type, bool> LogEnablements = new();

    private bool IsLogEnabled(Type hookType)
    {
        if (LogEnablements.TryGetValue(hookType, out var isLogEnabled))
        {
            return isLogEnabled;
        }

        isLogEnabled = hookType.GetCustomAttribute<DisableLogAttribute>() == null;
        LogEnablements.Add(hookType, isLogEnabled);

        return false;
    }

    private void LoggerTrace(Type hookType, string message, object? data = null)
    {
        if (!IsLogEnabled(hookType))
        {
            return;
        }

        Logger.Trace(message, data);
    }

    #endregion
}
