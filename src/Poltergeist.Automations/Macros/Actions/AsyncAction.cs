namespace Poltergeist.Automations.Macros;

public class AsyncAction : MacroAction
{
    public required Func<AsyncActionContext, Task> ExecuteAsync { get; set; }

    public string? ProgressTitle { get; set; }
}
