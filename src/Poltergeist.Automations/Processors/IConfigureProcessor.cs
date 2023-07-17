namespace Poltergeist.Automations.Processors;

public interface IConfigureProcessor : IProcessor
{
    public void SetOption(string key, object value);
}
