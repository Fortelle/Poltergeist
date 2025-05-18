namespace Poltergeist.Automations.Processors;

public class WorkflowStepFinallyArguments
{
    public required string StepId { get; init; }

    public required WorkflowStepResult Result { get; init; }

    public required DateTime StartTime { get; init; }

    public required DateTime EndTime { get; init; }

    public required TimeSpan Duration { get; init; }

    public string? NextStepId { get; set; }

    public object? Output { get; set; }

    public bool IsSucceeded => Result == WorkflowStepResult.Success;
}
