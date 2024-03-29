﻿using System.Diagnostics;
using Poltergeist.Automations.Logging;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Automations.Processors;

public class ArgumentService : IDisposable, IUserLogger
{
    public IUserProcessor Processor { get; }
    public IMacroBase Macro { get; }
    public IUserLogger Logger { get; }
    public OutputService Outputer => Processor.GetService<OutputService>();
    public string? Comment { get; set; }

    private string SenderName { get; }

    public ArgumentService(MacroProcessor processor)
    {
        Macro = processor.Macro;
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
