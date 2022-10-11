using System;
using System.Diagnostics;
using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Services;

public abstract class KernelService : IDisposable
{
    protected MacroProcessor Processor { get; }

    protected string ServiceName { get; }

    protected KernelService(MacroProcessor processor)
    {
        Processor = processor;

        ServiceName = GetType().Name;
    }

    public virtual void Dispose()
    {
        Debug.WriteLine($"Disposed {ServiceName}.");
    }
}
