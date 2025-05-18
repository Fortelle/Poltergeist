using System.Diagnostics;
using Poltergeist.Automations.Components.Logging;
using Poltergeist.Automations.Macros;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Services;

public abstract class MacroService : IExtensionService, IDisposable
{
    protected IServiceProcessor Processor { get; }

    protected LoggerWrapper Logger { get; }

    protected bool IsDisposed;

    private readonly string ServiceName;

    protected MacroService(MacroProcessor processor)
    {
        ServiceName = GetType().Name;

        Processor = processor;
        Logger = new(processor.GetService<MacroLogger>(), ServiceName);

        Logger.Debug($"Service '{ServiceName}' is instantiated.");
    }

    IUserProcessor IExtensionService.GetProcessor() => Processor;

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;
    }

    protected void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
    }

    public void Dispose()
    {
        Dispose(true);
        Debug.WriteLine($"Disposed {ServiceName}.");
        GC.SuppressFinalize(this);
    }
}

public abstract class MacroService<T>(MacroProcessor processor) : MacroService(processor) where T : MacroBase
{
    public T Macro => (T)Processor.Macro;
}
