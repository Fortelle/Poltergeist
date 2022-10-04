using System;
using System.Diagnostics;
using Poltergeist.Automations.Logging;

namespace Poltergeist.Automations.Processors;

public class ArgumentService : IDisposable, IUserLogger
{
    public IUserProcessor Processor { get; }
    public IUserLogger Logger { get; }

    private string SenderName { get; }

    public ArgumentService(MacroProcessor processor)
    {
        Processor = processor;
        Logger = this;

        SenderName = GetType().Name;
    }


    void IUserLogger.Trace(string message)
    {
        Processor.GetService<MacroLogger>().Log(LogLevel.Trace, SenderName, message);
    }

    void IUserLogger.Debug(string message)
    {
        Processor.GetService<MacroLogger>().Log(LogLevel.Debug, SenderName, message);
    }

    void IUserLogger.Info(string message)
    {
        Processor.GetService<MacroLogger>().Log(LogLevel.Information, SenderName, message);
    }

    void IUserLogger.Warn(string message)
    {
        Processor.GetService<MacroLogger>().Log(LogLevel.Warning, SenderName, message);
    }

    void IUserLogger.Error(string message)
    {
        Processor.GetService<MacroLogger>().Log(LogLevel.Error, SenderName, message);
    }

    void IUserLogger.Critical(string message)
    {
        Processor.GetService<MacroLogger>().Log(LogLevel.Critical, SenderName, message);
    }


    void IDisposable.Dispose()
    {
        Debug.WriteLine($"Disposed {SenderName}.");
    }
}
