using Poltergeist.Automations.Structures.Parameters;

namespace Poltergeist.Modules.Settings;

public class AppInternalSettings : SavablePredefinedCollection
{
    public Exception? Exception { get; }

    public AppInternalSettings()
    {
        AllowAbsentDefinition = true;
    }

    public AppInternalSettings(string path) : this()
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
