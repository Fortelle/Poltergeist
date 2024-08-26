namespace Poltergeist.Attributes;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public sealed class AssemblyGitHubUrlAttribute(string url) : Attribute
{
    public string Url => url;
}
