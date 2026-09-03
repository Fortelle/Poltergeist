namespace Poltergeist.Automations.Processors;

public interface IMacroProcessorInternal : IMacroProcessorInformation, IMacroProcessorShared
{
    void RaiseEvent(ProcessorEvent type, EventArgs eventArgs);

    void ReportException(Exception exception);
}
