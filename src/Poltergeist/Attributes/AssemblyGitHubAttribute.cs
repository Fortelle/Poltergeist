namespace Poltergeist.Attributes;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public sealed class AssemblyGitHubAttribute(string name) : Attribute
{
    public string Name => name;
}
