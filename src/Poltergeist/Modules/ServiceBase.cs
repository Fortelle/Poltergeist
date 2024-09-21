using Poltergeist.Modules.Logging;

namespace Poltergeist.Modules;

public abstract class ServiceBase
{
    protected AppLogWrapper Logger { get; }

    protected ServiceBase()
    {
        Logger = new AppLogWrapper(this);
        Logger.Trace($"Service '{GetType().Name}' is activated.");
    }
}
