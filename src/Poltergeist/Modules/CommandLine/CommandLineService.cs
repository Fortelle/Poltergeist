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

    public CommandLineService(AppEventService eventService)
    {
        eventService.Subscribe<AppContentLoadingEvent>(OnAppContentLoading);
        eventService.Subscribe<PipeMessageReceivedEvent>(OnPipeMessageReceived);
    }

    public void AddParser<T>() where T : CommandLineParser
    {
        ParserTypes.Add(typeof(T));
    }

    public static void Send(string[] options)
    {
        PipeService.Send(CommandLinePipeKey, options);
    }

    private void OnAppContentLoading(AppContentLoadingEvent _)
    {
        ParseOptions(PoltergeistApplication.Current.StartupOptions, false);
    }

    private void OnPipeMessageReceived(PipeMessageReceivedEvent e)
    {
        if (e.Message.Key != CommandLinePipeKey)
        {
            return;
        }

        if (e.Message.As<string[]>() is not string[] arguments)
        {
            return;
        }

        var options = new CommandLineOptionCollection(arguments);

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

}
