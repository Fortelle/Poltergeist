namespace Poltergeist.Modules.CommandLine;

public abstract class CommandLineParser
{
    public virtual bool AllowsPassed { get; set; }

    public abstract void Parse(CommandLineOptionArguments args);
}
