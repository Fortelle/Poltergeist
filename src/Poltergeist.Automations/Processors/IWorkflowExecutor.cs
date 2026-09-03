namespace Poltergeist.Automations.Processors;

public interface IWorkflowExecutor
{
    Task<bool> Process();
    void Cleanup();
}
