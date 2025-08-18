namespace Poltergeist.Modules.Navigation;

public class NavigationInfo
{
    public string PageKey { get; init; }
    public object? Argument { get; set; }

    public NavigationInfo(string pageKey)
    {
        PageKey = pageKey;
    }

    public NavigationInfo(string pageKey, object? argument)
    {
        PageKey = pageKey;
        Argument = argument;
    }
}
