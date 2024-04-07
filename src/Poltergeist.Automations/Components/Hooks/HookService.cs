using System.Collections.Concurrent;
using Poltergeist.Automations.Logging;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Components.Hooks;

public sealed class HookService : KernelService
{
    private Dictionary<Type, ConcurrentBag<Delegate>> Hooks { get; } = new();

    private MacroLogger Logger { get; }

    public HookService(MacroProcessor processor, MacroLogger logger) : base(processor)
    {
        Logger = logger;
    }

    public void Register<T>(Action handler) where T : MacroHook
    {
        Register(typeof(T), handler);
    }

    public void Register<T>(Action<T> handler) where T : MacroHook
    {
        Register(typeof(T), handler);
    }

    public void Register<T>(Action<T, IUserProcessor> handler) where T : MacroHook
    {
        Register(typeof(T), handler);
    }

    private void Register(Type type, Delegate handler)
    {
        if (!Hooks.TryGetValue(type, out var hook))
        {
            hook = new();
            Hooks.Add(type, hook);
        }

        hook.Add(handler);
        Logger.Log(LogLevel.Debug, nameof(HookService), $"A method is attached to hook <{type.Name}>.");
    }

    public void Raise<T>(T hook) where T : MacroHook
    {
        InternalRaise(hook);
    }

    public void RaiseUntil<T>(T hook, Func<T, bool> predicate) where T : MacroHook
    {
        InternalRaise(hook, predicate);
    }

    public void Raise<T>() where T : MacroHook, new()
    {
        InternalRaise(new T());
    }

    private void InternalRaise<T>(T hook, Func<T, bool>? untilPredicate = null) where T : MacroHook
    {
        var type = typeof(T);

        Logger.Log(LogLevel.Debug, nameof(HookService), $"Hook <{type.Name}> is triggered.");

        if (Hooks.TryGetValue(type, out var delegates))
        {
            foreach (var del in delegates)
            {
                // TODO: improve performance
                var paramCount = del.GetType().GetMethod("Invoke")!.GetParameters().Length;
                switch (paramCount)
                {
                    case 0:
                        del.DynamicInvoke();
                        break;
                    case 1:
                        del.DynamicInvoke(hook);
                        break;
                    case 2:
                        del.DynamicInvoke(hook, Processor);
                        break;
                    default:
                        throw new NotSupportedException();
                }

                if (untilPredicate?.Invoke(hook) == true)
                {
                    break;
                }
            }
        }
    }
}
