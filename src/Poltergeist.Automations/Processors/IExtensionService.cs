using Poltergeist.Automations.Components.Logging;

namespace Poltergeist.Automations.Processors;

public interface IExtensionService
{
    IUserProcessor GetProcessor();
    LoggerWrapper Logger { get; }
}
