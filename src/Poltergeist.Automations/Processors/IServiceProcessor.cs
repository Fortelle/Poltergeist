using Poltergeist.Automations.Macros;

namespace Poltergeist.Automations.Processors;

public interface IServiceProcessor : IProcessor
{
    public IMacroBase Macro { get; }
    public string ProcessId { get; }

    public DateTime StartTime { get; }
    public DateTime EndTime { get; }
    public bool IsCancelled { get; }

    public void SetStatistic<T>(string key, T value) where T : notnull;
    public void SetStatistic<T>(string key, Func<T, T> action) where T : notnull;
    public void RaiseEvent(MacroEventType type, EventArgs eventArgs);
    public void RaiseAction(Action action);
    public Task Pause();
    public void Resume();

}
