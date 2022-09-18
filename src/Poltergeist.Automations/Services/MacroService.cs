using System;
using System.Diagnostics;
using Poltergeist.Automations.Logging;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Services;

public abstract class MacroService : IDisposable
{
    public MacroProcessor Processor { get; }

    private string SenderName { get; }

    private MacroLogger _logger;
    private MacroLogger Logger => _logger ??= Processor.GetService<MacroLogger>();

    protected MacroService(MacroProcessor processor)
    {
        Processor = processor;

        SenderName = this.GetType().Name;
    }

    protected virtual void Log(LogLevel level, string message, params object[] args)
    {
        Logger.Log(level, SenderName, message, args);
    }

    public virtual void Dispose()
    {
        Debug.WriteLine($"Disposed {SenderName}.");
    }

}

public abstract class MacroService<T> : MacroService
    where T : MacroBase
{
    public T Macro => Processor.Macro as T;

    protected MacroService(MacroProcessor processor) : base(processor)
    {
    }

}
