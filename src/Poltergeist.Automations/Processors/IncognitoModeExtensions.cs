namespace Poltergeist.Automations.Processors;

public static class IncognitoModeExtensions
{
    public static string Key { get; } = "incognito_mode";

    public static bool IsIncognitoMode(this IProcessor processor)
    {
        return processor.Environments.Get<bool>(Key) == true;
    }
}
