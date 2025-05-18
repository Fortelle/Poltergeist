using System.Diagnostics.CodeAnalysis;

namespace Poltergeist.Automations.Processors;

public class WorkflowStep
{
    public required string Id { get; set; }

    public required Func<WorkflowStepArguments, bool> Action { get; set; }

    public Action<WorkflowStepFinallyArguments>? Finally { get; set; }

    public string? SuccessStepId { get; set; }

    public string? FailureStepId { get; set; }

    public string? ErrorStepId { get; set; }

    public string? InterruptionStepId { get; set; }

    public bool IsDefault { get; set; }

    public bool IsInterruptable { get; set; }

    [SetsRequiredMembers]
    public WorkflowStep(string id, Func<WorkflowStepArguments, bool> action)
    {
        Id = id;
        Action = action;
    }

    [SetsRequiredMembers]
    public WorkflowStep(string id, Action<WorkflowStepArguments> action)
    {
        Id = id;
        Action = e =>
        {
            action(e);
            return true;
        };
    }

    [SetsRequiredMembers]
    public WorkflowStep(string id, Action action)
    {
        Id = id;
        Action = _ =>
        {
            action();
            return true;
        };
    }
}
