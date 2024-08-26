namespace Poltergeist.Attributes;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public sealed class AssemblyLicenseAttribute(string licence) : Attribute
{
    public string License => licence;
}
