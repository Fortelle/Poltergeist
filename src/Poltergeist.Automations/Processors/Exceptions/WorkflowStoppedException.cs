namespace Poltergeist.Automations.Processors;

public class WorkflowStoppedException : Exception
{
    public WorkflowStoppedException() : base()
    {

    }

    public static void ThrowIf(bool condition)
    {
        if (condition)
        {
            throw new WorkflowStoppedException();
        }
    }
}