namespace Poltergeist.ViewModels;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public sealed class AssemblyLicenseUrlAttribute : Attribute
{
    public string Url { get; }

    public AssemblyLicenseUrlAttribute(string url)
    {
        Url = url;
    }
}
