using Poltergeist.Modules.Navigation;

namespace Poltergeist.Tests.TestTasks;

public abstract class TestTask
{
    public virtual string? Title { get; }

    public virtual string? Description { get; }

    public virtual PageInfo? PageInfo { get; }

    public virtual Action? Execute { get; }

    public virtual Func<Task>? ExecuteAsync { get; }
}
