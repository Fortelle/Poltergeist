namespace Poltergeist.ViewModels;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public sealed class AssemblyGitHubUrlAttribute : Attribute
{
    public string Url { get; }

    public AssemblyGitHubUrlAttribute(string url)
    {
        Url = url;
    }
}
