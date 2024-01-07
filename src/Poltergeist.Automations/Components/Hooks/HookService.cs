using System.Collections.Concurrent;
using System.Diagnostics;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Components.Hooks;

public sealed class HookService : KernelService
{
    private Dictionary<Type, ConcurrentBag<Delegate>> Hooks { get; } = new();

    public HookService(MacroProcessor processor) : base(processor)
    {
    }

    public void Register<T>(Action handler) where T : MacroHook
    {
        Register(typeof(T), handler);
    }

    public void Register<T>(Action<T> handler) where T : MacroHook
    {
        Register(typeof(T), handler);
    }

    public void Register<T>(Action<T, MacroProcessor> handler) where T : MacroHook
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
        Debug.WriteLine($"A method is registered to hook \"{type.Name}\".");
    }

    public void Raise<T>(T hook) where T : MacroHook
    {
        var type = typeof(T);

        Debug.WriteLine($"Triggered \"{type.Name}\".");

        if (Hooks.TryGetValue(type, out var delegates))
        {
            foreach(var del in delegates)
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
            }
        }
    }

    public void Raise<T>() where T : MacroHook, new()
    {
        Raise(new T());
    }

}
