namespace Poltergeist.ViewModels;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public sealed class AssemblyLicenseAttribute : Attribute
{
    public string License { get; }

    public AssemblyLicenseAttribute(string licence)
    {
        License = licence;
    }
}
