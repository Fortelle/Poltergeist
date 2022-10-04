namespace Poltergeist.Automations.Processors;

public interface IUserProcessor
{
    public T GetService<T>() where T : class;
    public T GetOption<T>(string key, T def = default);
    public T GetEnvironment<T>(string key, T def = default);
}

public interface IConfigureProcessor
{
    public T GetService<T>() where T : class;
    public T GetOption<T>(string key, T def = default);
    public T GetEnvironment<T>(string key, T def = default);
}
