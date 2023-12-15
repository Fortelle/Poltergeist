using Poltergeist.Automations.Parameters;

namespace Poltergeist.Automations.Processors;

public interface IUserProcessor : IProcessor
{
    public bool IsCancelled { get; }
    public CancellationToken? CancellationToken { get; }

    public VariableCollection SessionStorage { get; }

    public Task Pause();
    public void Resume();
}
