namespace Poltergeist.ViewModels;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public sealed class AssemblyGitHubAttribute : Attribute
{
    public string Name { get; }

    public AssemblyGitHubAttribute(string name)
    {
        Name = name;
    }
}
