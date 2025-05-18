using System.Diagnostics;
using Poltergeist.Automations.Components.Logging;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Automations.Processors;

public class ArgumentService : IDisposable, IUserLogger
{
    public IUserProcessor Processor { get; }
    public string? Comment { get; set; }

    public IUserLogger Logger => this; // todo: change to wrapper
    public OutputService Outputer => Processor.GetService<OutputService>();
    public IUserMacro Macro => Processor.Macro;

    private readonly string SenderName;
    protected bool IsDisposed;

    public ArgumentService(MacroProcessor processor)
    {
        Processor = processor;

        SenderName = GetType().Name;
    }

    void IUserLogger.Log(string message)
    {
        Processor.GetService<MacroLogger>().Log(LogLevel.Information, SenderName, message);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        Debug.WriteLine($"Disposed {SenderName}.");
        GC.SuppressFinalize(this);
    }
}
