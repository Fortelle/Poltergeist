using Poltergeist.Automations.Components.Logging;

namespace Poltergeist.Automations.Processors;

public interface IMacroServiceShared
{
    IMacroProcessorShared GetProcessor();
    LoggerWrapper Logger { get; }
}
