using System;
using System.Threading.Tasks;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Automations.Processors;

public interface IProcessor
{
    public T GetService<T>() where T : class;
    public T GetOption<T>(string key, T def = default);
    public T GetEnvironment<T>(string key, T def = default);
}

public interface IUserProcessor : IProcessor
{
}

public interface IConfigureProcessor : IProcessor
{
}

public interface IServiceProcessor : IProcessor
{
    public IMacroBase Macro { get; }

    public DateTime StartTime { get; }
    public DateTime EndTime { get; }

    public void SetStatistic<T>(string key, T value);
    public void SetStatistic<T>(string key, Func<T, T> action);
    public void RaiseEvent(MacroEventType type, EventArgs eventArgs);
    public void RaiseAction(Action action);
    public Task Pause();
    public void Resume();
}

public interface IExtensionService
{
    public IUserProcessor GetProcessor();
}
