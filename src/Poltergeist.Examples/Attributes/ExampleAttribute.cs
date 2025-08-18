namespace Poltergeist.Examples;

[AttributeUsage(AttributeTargets.Class)]
public class ExampleMacroAttribute : Attribute
{
    public bool IsIncognito { get; set; }
}
