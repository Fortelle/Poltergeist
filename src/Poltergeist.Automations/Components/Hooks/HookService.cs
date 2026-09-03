using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Poltergeist.Automations.Components.Logging;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Components.Hooks;

public sealed class HookService : KernelService
{
    private readonly Dictionary<string, HookDelegator> Delegators = new();

    private readonly LoggerWrapper Logger;

    public HookService(MacroProcessor processor, MacroLogger loggerService) : base(processor)
    {
        Logger = new(loggerService, nameof(HookService));

        Logger.Debug($"Kernel service {nameof(HookService)} is instantiated.");
    }

    public void Register<THook>(Action<IMacroProcessorShared, THook> handler, bool once = false, int priority = 0) where THook : MacroHook
    {
        var hookKey = ConvertTypeToKey(typeof(THook));
        InternalRegister(new HookListener(hookKey, handler)
        {
            Priority = priority,
            Once = once,
#if DEBUG
            Subscriber = GetCallingClassName(2),
#endif
        });
    }

    public void Register<THook>(Func<IMacroProcessorShared, THook, Task> handler, bool once = false, int priority = 0) where THook : MacroHook
    {
        var hookKey = ConvertTypeToKey(typeof(THook));
        InternalRegister(new HookListener(hookKey, handler)
        {
            Priority = priority,
            Once = once,
#if DEBUG
            Subscriber = GetCallingClassName(2),
#endif
        });
    }

    public void Register(HookListener listener)
    {
        InternalRegister(listener);
    }

    public void Raise<THook>(THook hook) where THook : MacroHook
    {
        var hookKey = ConvertTypeToKey(typeof(THook));
        InternalRaise(hookKey, [Processor, hook]);
    }

    public void Raise<THook>() where THook : MacroHook, new()
    {
        var hookKey = ConvertTypeToKey(typeof(THook));
        var hook = new THook();
        InternalRaise(hookKey, [Processor, hook]);
    }

    public Task RaiseAsync<THook>(THook hook) where THook : MacroHook
    {
        Processor.ThrowIfCancellationRequested();

        var hookKey = ConvertTypeToKey(typeof(THook));
        return InternalRaiseAsync(hookKey, [Processor, hook]);
    }

    public Task RaiseAsync<THook>() where THook : MacroHook, new()
    {
        Processor.ThrowIfCancellationRequested();

        var hookKey = ConvertTypeToKey(typeof(THook));
        var hook = new THook();
        return InternalRaiseAsync(hookKey, [Processor, hook]);
    }

    public void Unregister<THook>(Action<IMacroProcessorShared, THook> handler) where THook : MacroHook
    {
        InternalUnregister(ConvertTypeToKey(typeof(THook)), handler);
    }

    public void Unregister<THook>(Func<IMacroProcessorShared, THook, Task> handler) where THook : MacroHook
    {
        InternalUnregister(ConvertTypeToKey(typeof(THook)), handler);
    }

    public void Register(string hookKey, Delegate handler, bool once = false, int priority = 0)
    {
        InternalRegister(new HookListener(hookKey, handler)
        {
            Priority = priority,
            Once = once,
#if DEBUG
            Subscriber = GetCallingClassName(2),
#endif
        });
    }

    public void Raise(string hookKey, params object[] arguments)
    {
        InternalRaise(hookKey, arguments);
    }

    public Task RaiseAsync(string hookKey, params object[] arguments)
    {
        Processor.ThrowIfCancellationRequested();

        return InternalRaiseAsync(hookKey, arguments);
    }

    public void Unregister(string hookKey, Delegate handler)
    {
        InternalUnregister(hookKey, handler);
    }

    public void RegisterMethods(object obj)
    {
        var objectType = obj.GetType();
        foreach (var method in objectType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy))
        {
            var attr = method.GetCustomAttribute<MacroHookAttribute>();
            if (attr is null)
            {
                continue;
            }

            if (attr.Key is null)
            {
                var methodParameters = method.GetParameters();
                if (methodParameters.Length != 2)
                {
                    throw new ArgumentException($"The hook method '{method.DeclaringType?.Name ?? objectType.Name}.{method.Name}' does not have the corrent signature.");
                }
                var processorType = methodParameters[0].ParameterType;
                if (typeof(IMacroProcessorShared).IsAssignableFrom(processorType) == false)
                {
                    throw new ArgumentException($"The hook method '{method.DeclaringType?.Name ?? objectType.Name}.{method.Name}' does not have the corrent signature.");
                }
                var hookType = methodParameters[1].ParameterType;
                if (typeof(MacroHook).IsAssignableFrom(hookType) == false)
                {
                    throw new ArgumentException($"The hook method '{method.DeclaringType?.Name ?? objectType.Name}.{method.Name}' does not have the corrent signature.");
                }
                var handlerType = Expression.GetDelegateType([processorType, hookType, method.ReturnType]);
                var handler = method.IsStatic
                    ? Delegate.CreateDelegate(handlerType, method)
                    : method.CreateDelegate(handlerType, obj);
                var hookKey = ConvertTypeToKey(hookType);
                InternalRegister(new HookListener(hookKey, handler)
                {
                    Priority = attr.Priority,
                    Once = attr.Once,
                    Subscriber = objectType.Name,
                    MethodName = method.Name,
                });
            }
            else
            {
                var typeArgs = method.GetParameters().Select(parm => parm.ParameterType).Append(method.ReturnType).ToArray();
                var handlerType = Expression.GetDelegateType(typeArgs);
                var handler = method.IsStatic
                    ? Delegate.CreateDelegate(handlerType, method)
                    : method.CreateDelegate(handlerType, obj);
                InternalRegister(new HookListener(attr.Key, handler)
                {
                    Priority = attr.Priority,
                    Once = attr.Once,
                    Subscriber = objectType.Name,
                    MethodName = method.Name,
                });
            }
        }
    }

    private void InternalRegister(HookListener listener)
    {
        if (!Delegators.TryGetValue(listener.Key, out var delegator))
        {
            delegator = new HookDelegator(listener.Key);
            Delegators.Add(listener.Key, delegator);
        }

        delegator.Listeners.Add(listener);

        Logger.Trace($"A listener is registered to hook '{delegator.Key}'.", new { Hook = delegator.Key, listener.Subscriber, listener.MethodName });
    }

    private void InternalRaise(string hookKey, object[] arguments)
    {
        Delegators.TryGetValue(hookKey, out var delegator);

        Logger.Trace($"Hook '{hookKey}' is triggered.", new { Listeners = delegator?.Listeners.Count ?? 0 });

        if (delegator is null || delegator.Listeners.Count == 0)
        {
            return;
        }

        var listeners = delegator.Listeners.OrderByDescending(x => x.Priority).ToArray();

        Logger.IncreaseIndent();

        foreach (var listener in listeners)
        {
            Logger.Trace($"Executing the callback.", new { Hook = delegator.Key, listener.Subscriber, listener.MethodName, listener.Once });
            Logger.IncreaseIndent();

            var result = listener.Callback.DynamicInvoke(arguments);
            if (result is Task task)
            {
                task.GetAwaiter().GetResult();
            }

            if (listener.Once)
            {
                delegator.Listeners.Remove(listener);
            }

            Logger.DecreaseIndent();
        }

        Logger.DecreaseIndent();
    }

    private async Task InternalRaiseAsync(string hookKey, object[] arguments)
    {
        Processor.ThrowIfCancellationRequested();

        Delegators.TryGetValue(hookKey, out var delegator);

        Logger.Trace($"Hook '{hookKey}' is triggered.", new { Listeners = delegator?.Listeners.Count ?? 0 });

        if (delegator is null || delegator.Listeners.Count == 0)
        {
            return;
        }

        var listeners = delegator.Listeners.OrderByDescending(x => x.Priority).ToArray();

        Logger.IncreaseIndent();

        foreach (var listener in listeners)
        {
            Processor.ThrowIfCancellationRequested();

            Logger.Trace($"Executing the callback asynchronously.", new { Hook = delegator.Key, listener.Subscriber, listener.MethodName, listener.Once });
            Logger.IncreaseIndent();

            var result = listener.Callback.DynamicInvoke(arguments);
            if (result is Task task)
            {
                await task;
            }

            if (listener.Once)
            {
                delegator.Listeners.Remove(listener);
            }

            Logger.DecreaseIndent();
        }

        Logger.DecreaseIndent();
    }

    private void InternalUnregister(string hookKey, Delegate del)
    {
        if (!Delegators.TryGetValue(hookKey, out var delegator))
        {
            return;
        }

        var listener = delegator.Listeners.FirstOrDefault(x => x.Callback == del);

        if (listener is null)
        {
            return;
        }

        delegator.Listeners.Remove(listener);

        Logger.Trace($"A listener is removed from hook '{delegator.Key}'.", new { Hook = delegator.Key, listener.Subscriber, listener.MethodName });
    }

    private static string? GetCallingClassName(int skipFrames)
    {
        return new StackFrame(skipFrames, false).GetMethod()?.DeclaringType?.Name;
    }

    public static string ConvertTypeToKey(Type hookType)
    {
        var key = hookType.Name;
        if (key.EndsWith("Hook"))
        {
            key = key[0..^4];
        }
        return key;
    }

}
