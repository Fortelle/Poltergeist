namespace Poltergeist.Automations.Macros;

public class TerminalActionContext : ActionContext
{
    public CancellationToken CancellationToken { get; set; }

    public event Action<string>? Logged;

    public event Action<double, string?>? ProgressChanged;

    public void Log(string message)
    {
        Logged?.Invoke(message);
    }

    public void UpdateProgress(double value, string? message = null)
    {
        ProgressChanged?.Invoke(value, message);
    }
}
