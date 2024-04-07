using System.Diagnostics;
using Poltergeist.Automations.Logging;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Automations.Processors;

public class ArgumentService : IDisposable, IUserLogger
{
    public IUserProcessor Processor { get; }
    public string? Comment { get; set; }

    public IUserLogger Logger => this;
    public OutputService Outputer => Processor.GetService<OutputService>();
    public IUserMacro Macro => Processor.Macro;

    private readonly string SenderName;

    public ArgumentService(MacroProcessor processor)
    {
        Processor = processor;

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
