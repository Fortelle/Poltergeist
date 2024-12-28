namespace Poltergeist.Modules.CommandLine;

public class CommandLineOptionCollection
{
    private readonly List<CommandLineOption> Options = new();

    public CommandLineOptionCollection()
    {
    }

    public string this[string name]
    {
        get
        {
            return Get(name)?.Value ?? throw new KeyNotFoundException();
        }
    }

    public void Add(string name, string? value)
    {
        name = CommandLineService.NormalizeName(name)!;
        Options.Add(new CommandLineOption(name, value));
    }

    public CommandLineOption? Get(string name)
    {
        name = NormalizeName(name);
        return Options.FirstOrDefault(x => x.Name == name);
    }

    public bool Contains(string name)
    {
        name = NormalizeName(name);
        return Options.Any(x => x.Name == name);
    }

    private static string NormalizeName(string name)
    {
        return CommandLineService.NormalizeName(name)!;
    }
}
