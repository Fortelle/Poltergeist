namespace Poltergeist.Automations.Processors;

public static class IncognitoModeExtensions
{
    public const string EnvironmentKey = "is_incognitomode";

    public static bool IsIncognitoMode(this IProcessor processor)
    {
        return processor.Environments.GetValueOrDefault<bool>(EnvironmentKey) == true;
    }
}
