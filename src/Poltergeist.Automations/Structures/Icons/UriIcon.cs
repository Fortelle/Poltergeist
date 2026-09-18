namespace Poltergeist.Automations.Structures;

public class UriIcon : IconInfo
{
    public string Uri { get; }

    public UriIcon(string uri)
    {
        Uri = uri;
    }

    public override string ToString() => Uri;
}
