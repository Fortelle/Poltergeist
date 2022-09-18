using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Poltergeist.Automations.Logging;
using Poltergeist.Automations.Services;

namespace Poltergeist.Automations.Processors;

public sealed class HookService : MacroService
{
    public delegate void HookHandler(object[] args);

    private Dictionary<string, HookHandler> Hooks { get; } = new();

    public HookService(MacroProcessor processor) : base(processor)
    {
    }

    public void Register(string eventName, HookHandler handler)
    {
        eventName = eventName.ToLower();

        if (!Hooks.ContainsKey(eventName))
        {
            Hooks.Add(eventName, null);
        }

        Hooks[eventName] += handler;

        Log(LogLevel.Debug, $"A method is registered to hook \"{eventName}\".");
    }

    public void Raise(string eventName, params object[] args)
    {
        eventName = eventName.ToLower();
        Log(LogLevel.Debug, $"Hook \"{eventName}\" is triggered.");

        if (Hooks.TryGetValue(eventName, out var hooker))
        {
            hooker.Invoke(args);
        }
    }

}

// todo
public enum HookLifetime
{
    Alive,
    OneOff,
    KeepFiring,
}
