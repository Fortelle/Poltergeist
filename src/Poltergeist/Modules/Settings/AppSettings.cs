using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Modules.Settings;

public class AppSettings : SavablePredefinedCollection
{
    public Exception? Exception { get; }

    public AppSettings()
    {
    }

    public AppSettings(string path)
    {
        try
        {
            Load(path);
        }
        catch (FileNotFoundException)
        {
        }
        catch (Exception exception)
        {
            Exception = exception;
        }
    }
}
