using Poltergeist.Automations.Components.Interactions;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Automations.Processors;

public interface IUserProcessor : IProcessor
{
    public IUserMacro Macro { get; }

    public bool IsCancelled { get; }
    public CancellationToken CancellationToken { get; }

    public Task Pause(PauseReason reason);
    public void Resume();

    public T GetService<T>() where T : class;
    public void Interact(InteractionModel model);
    public void ThrowIfInterrupted();
}
