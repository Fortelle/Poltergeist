using System;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
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


    void IUserLogger.Log(string message)
    {
        Processor.GetService<MacroLogger>().Log(LogLevel.Information, SenderName, message);
    }


    void IDisposable.Dispose()
    {
        Debug.WriteLine($"Disposed {SenderName}.");
    }

}
