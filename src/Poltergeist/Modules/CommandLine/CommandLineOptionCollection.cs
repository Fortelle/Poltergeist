namespace Poltergeist.Modules.CommandLine;

public class CommandLineOptionCollection
{
    private readonly List<CommandLineOption> Options = new();

    public CommandLineOptionCollection()
    {
    }

    public CommandLineOptionCollection(string[] args)
    {
        Load(args);
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
        name = NormalizeName(name)!;
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

    private void Load(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            var option = "";
            var value = default(string?);
            if (args[i].StartsWith("--"))
            {
                var parts = args[i][2..].Split('=', 2);
                option = parts[0];
                if (parts.Length == 2)
                {
                    value = parts[1];
                }
            }
            else if (args[i].StartsWith('/'))
            {
                var parts = args[i][1..].Split(':', 2);
                option = parts[0];
                if (parts.Length == 2)
                {
                    value = parts[1];
                }
            }
            if (value is null && i < args.Length - 1 && !args[i + 1].StartsWith('-') && !args[i + 1].StartsWith('/'))
            {
                value = args[i + 1];
                i++;
            }
            option = NormalizeName(option);
            Add(option, value);
        }
    }

    private static string NormalizeName(string name)
    {
        return name
            .ToLower()
            .Replace(" ", "")
            .Replace("-", "")
            .Replace("_", "")
            ;
    }

}
