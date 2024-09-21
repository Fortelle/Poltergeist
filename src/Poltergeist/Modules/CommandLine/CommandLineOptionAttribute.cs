namespace Poltergeist.Modules.CommandLine;

[AttributeUsage(AttributeTargets.Property)]
public class CommandLineOptionAttribute : Attribute
{
    public string? LongName { get; set; }

    public char? ShortName { get; set; }
}
