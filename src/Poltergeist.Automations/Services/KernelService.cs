using System.Diagnostics;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Services;

public abstract class KernelService : IDisposable
{
    protected MacroProcessor Processor;

    protected bool IsDisposed;

    private readonly string ServiceName;

    protected KernelService(MacroProcessor processor)
    {
        Processor = processor;

        ServiceName = GetType().Name;
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
        Debug.WriteLine($"Disposed {ServiceName}.");
        GC.SuppressFinalize(this);
    }
}
