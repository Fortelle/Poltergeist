using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Macros.Oneshots;

public class OneshotMacro : CommonOneshotMacroBase
{
    public Action<WorkflowController>? Execute { get; set; }

    public Func<WorkflowController, Task>? ExecuteAsync { get; set; }

    public OneshotMacro(string? name = null) : base(name)
    {
    }

    protected override void OnExecute(WorkflowController controller)
    {
        if (Execute is not null)
        {
            Execute(controller);
        }
    }

    protected async override Task OnExecuteAsync(WorkflowController controller)
    {
        if (ExecuteAsync is not null)
        {
            await ExecuteAsync(controller);
        }
    }
}
