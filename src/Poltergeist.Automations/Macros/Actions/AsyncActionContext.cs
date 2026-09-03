namespace Poltergeist.Automations.Macros;

public class AsyncActionContext : ActionContext
{
    public string? Message { get; set; }

    public CancellationToken CancellationToken { get; set; }
}
