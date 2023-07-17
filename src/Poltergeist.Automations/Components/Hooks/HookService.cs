using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

    public void Register<T>(Action<T> handler) where T : MacroHook
    {
        var type = typeof(T);

        if (!Hooks.ContainsKey(type))
        {
            Hooks.Add(type, new());
        }

        Hooks[type].Add(handler);
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
                del.DynamicInvoke(hook);
            }
        }
    }

    public void Raise<T>() where T : MacroHook, new()
    {
        Raise(new T());
    }

}
