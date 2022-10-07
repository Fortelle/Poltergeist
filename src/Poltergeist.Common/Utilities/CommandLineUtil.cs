using System;
using System.Collections.Generic;
using System.Linq;

namespace Poltergeist.Common.Utilities;

public static class CommandLineUtil
{
    public static CommandOption[] GetOptions(string[] args)
    {
        var list = new List<CommandOption>();
        for (var i = 0; i < args.Length; i++)
        {
            string option = "";
            string? value = null;
            if (args[i].StartsWith("--"))
            {
                var parts = args[i].Substring(2).Split('=', 2);
                option = parts[0];
                if (parts.Length == 2) value = parts[1];
            }
            else if (args[i].StartsWith('/'))
            {
                var parts = args[i].Substring(1).Split(':', 2);
                option = parts[0];
                if (parts.Length == 2) value = parts[1];
            }
            if (value == null && i < args.Length - 1 && !args[i + 1].StartsWith('-') && !args[i + 1].StartsWith('/'))
            {
                value = args[i + 1];
                i++;
            }
            list.Add(new CommandOption()
            {
                Name = option,
                Value = value ?? ""
            });
        }
        return list.ToArray();
    }

}

public class CommandOption
{
    public string Name { get; set; }
    public string Value { get; set; }
}
