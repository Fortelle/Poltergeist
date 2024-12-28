namespace Poltergeist.Modules.CommandLine;

public class CommandLineOptionArguments
{
    public required CommandLineOptionCollection Options { get; init; }

    public bool IsPassed { get; init; }
}
