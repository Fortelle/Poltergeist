namespace Poltergeist.Common.Utilities;

/// <summary>
/// Provides a simple way to parse command line options.
/// </summary>
public static class CommandLineUtil
{
    /// <summary>
    /// Gets the command options from the specified arguments.
    /// </summary>
    /// <param name="args">An array of arguments.</param>
    /// <returns>An array of <see cref="CommandOption"/> objects.</returns>
    public static CommandOption[] GetOptions(string[] args)
    {
        var list = new List<CommandOption>();
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
            if (value == null && i < args.Length - 1 && !args[i + 1].StartsWith('-') && !args[i + 1].StartsWith('/'))
            {
                value = args[i + 1];
                i++;
            }
            list.Add(new CommandOption(option, value));
        }
        return list.ToArray();
    }

}
