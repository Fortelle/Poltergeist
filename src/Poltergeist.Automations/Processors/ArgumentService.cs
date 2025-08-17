using System.Diagnostics;
using Poltergeist.Automations.Components.Logging;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Automations.Processors;

public class ArgumentService : IDisposable
{
    public IUserProcessor Processor { get; }
    public string? Comment { get; set; }

    public LoggerWrapper Logger { get; }
    public OutputService Outputer => Processor.GetService<OutputService>();
    public IUserMacro Macro => Processor.Macro;

    private readonly string SenderName;
    protected bool IsDisposed;

    public ArgumentService(MacroProcessor processor)
    {
        Processor = processor;

        SenderName = GetType().Name;

        Logger = new(processor.GetService<MacroLogger>(), SenderName);
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
