using System.Reflection;
using Poltergeist.Helpers;
using Poltergeist.Modules.App;
using Poltergeist.Modules.Events;
using Poltergeist.Modules.Pipes;

namespace Poltergeist.Modules.CommandLine;

public class CommandLineService : ServiceBase
{
    private const string CommandLinePipeKey = "commandline";

    private static readonly List<Type> ParserTypes = new();

    private static CommandLineOptionCollection? _startupOptions;
    public static CommandLineOptionCollection StartupOptions
    {
        get
        {
            if (_startupOptions is null)
            {
                var args = Environment.GetCommandLineArgs();
                _startupOptions = GetOptions(args[1..]);
            }

            return _startupOptions;
        }
    }

    public CommandLineService(AppEventService eventService)
    {
        eventService.Subscribe<AppContentLoadingHandler>(OnAppContentLoading);
        eventService.Subscribe<PipeMessageReceivedHandler>(OnPipeMessageReceived);
    }

    public void AddParser<T>() where T : CommandLineParser
    {
        ParserTypes.Add(typeof(T));
    }

    public static void Send(string[] options)
    {
        PipeService.Send(CommandLinePipeKey, options);
    }

    private void OnAppContentLoading(AppContentLoadingHandler e)
    {
        ParseOptions(StartupOptions, false);
    }

    private void OnPipeMessageReceived(PipeMessageReceivedHandler e)
    {
        if (e.Message.Key != CommandLinePipeKey)
        {
            return;
        }

        if (e.Message.As<string[]>() is not string[] arguments)
        {
            return;
        }

        var options = GetOptions(arguments);

        ApplicationHelper.BringToFront();

        PoltergeistApplication.TryEnqueue(() =>
        {
            ParseOptions(options, true);
        });
    }

    private void ParseOptions(CommandLineOptionCollection options, bool isPassed)
    {
        Logger.Trace($"Parsing command line options.", new { options });

        foreach (var parserType in ParserTypes)
        {
            var parser = CreateParser(parserType, options);
            var handler = new CommandLineOptionArguments()
            {
                Options = options,
                IsPassed = isPassed,
            };
            parser.Parse(handler);
            Logger.Trace($"Executed parser '{parserType.Name}'.", new { parser, isPassed });
        }

        Logger.Debug($"Parsed command line options.");
    }

    private static CommandLineParser CreateParser(Type parserType, CommandLineOptionCollection options)
    {
        var parser = (CommandLineParser)Activator.CreateInstance(parserType)!;

        foreach (var property in parserType.GetProperties())
        {
            var attr = property.GetCustomAttribute<CommandLineOptionAttribute>(true);
            if (attr is null)
            {
                continue;
            }

            var option = options.Get(attr.LongName ?? property.Name);
            if (option is null && attr.ShortName is not null)
            {
                option = options.Get(attr.ShortName.ToString()!);
            }
            if (option is null)
            {
                continue;
            }

            var valueType = property.PropertyType;
            if (valueType == typeof(bool))
            {
                property.SetValue(parser, true);
            }
            else if (string.IsNullOrEmpty(option.Value))
            {
                var value = Activator.CreateInstance(valueType);
                property.SetValue(parser, value);
            }
            else
            {
                var value = Convert.ChangeType(option.Value, valueType);
                property.SetValue(parser, value);
            }
        }

        return parser;
    }

    public static string? NormalizeName(string? name)
    {
        return name?
            .ToLower()
            .Replace(" ", "")
            .Replace("_", "")
            .Replace("_", "")
            ;
    }

    private static CommandLineOptionCollection GetOptions(string[] args)
    {
        var options = new CommandLineOptionCollection();
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
            options.Add(option, value);
        }
        return options;
    }

}
