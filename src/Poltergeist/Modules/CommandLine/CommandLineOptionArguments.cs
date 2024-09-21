namespace Poltergeist.Modules.CommandLine;

public class CommandLineOptionArguments
{
    public required CommandLineOption[] Options { get; init; }

    public bool IsPassed { get; init; }
}
