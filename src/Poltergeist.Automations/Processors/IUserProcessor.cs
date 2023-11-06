namespace Poltergeist.Automations.Processors;

public interface IUserProcessor : IProcessor
{
    public bool IsCancelled { get; }
    public CancellationToken? CancellationToken { get; }

    public CacheStorage Caches { get; }

    public Task Pause();
    public void Resume();
}
