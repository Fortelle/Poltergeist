namespace Poltergeist.Automations.Processors;

public class WorkflowStepArguments
{
    public required string StepId { get; init; }

    public required DateTime StartTime { get; init; }

    public WorkflowStepReport? PreviousResult { get; init; }

    public object? Output { get; set; }

    public string? SuccessStepId { get; set; }

    public string? FailureStepId { get; set; }

    public string? ErrorStepId { get; set; }

    public string? InterruptionStepId { get; set; }
}
