namespace Poltergeist.Attributes;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public sealed class AssemblyLicenseUrlAttribute(string url) : Attribute
{
    public string Url => url;
}
