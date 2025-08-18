using Poltergeist.Automations.Macros;

namespace Poltergeist.Automations.Processors;

public interface IUserProcessor : IProcessor
{
    IUserMacro Macro { get; }

    bool IsCancelled { get; }
    CancellationToken CancellationToken { get; }

    Task Pause(PauseReason reason);
    void Resume();

    T GetService<T>() where T : class;
    void ThrowIfInterrupted();
}
