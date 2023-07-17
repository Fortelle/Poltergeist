using System;

namespace Poltergeist.Automations.Processors;

public interface IProcessor
{
    public T GetService<T>() where T : class;
    public T? GetOption<T>(string key, T? def = default);
    public object? GetOption(string key, Type type);
    public T? GetEnvironment<T>(string key, T? def = default);
}
