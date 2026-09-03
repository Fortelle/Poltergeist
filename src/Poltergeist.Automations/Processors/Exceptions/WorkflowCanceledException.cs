namespace Poltergeist.Automations.Processors;

public class WorkflowCanceledException : Exception
{
    public WorkflowCanceledException() : base()
    {

    }

    public static void ThrowIf(bool condition)
    {
        if (condition)
        {
            throw new WorkflowCanceledException();
        }
    }
}
