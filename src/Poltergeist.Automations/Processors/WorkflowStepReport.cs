namespace Poltergeist.Automations.Processors;

public class WorkflowStepReport
{
    public required string StepId { get; init; }

    public WorkflowStepResult Result { get; init; }

    public string? NextStepId { get; init; }

    public object? Output { get; init; }
}
