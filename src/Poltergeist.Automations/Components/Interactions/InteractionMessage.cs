using System.Text;

namespace Poltergeist.Automations.Components.Interactions;

public class InteractionMessage
{
    public const string MacroKeyName = "macro_key";
    public const string ProcessIdName = "process_id";
    private const char Separater = ';';

    public string? MacroKey { get; set; }
    public string? ProcessId { get; set; }

    private Dictionary<string, string> Arguments { get; } = new();

    public string this[string key]
    {
        get => Arguments[key];
        set => Arguments[key] = value;
    }

    public static implicit operator string(InteractionMessage message) => message.ToArgument();

    public InteractionMessage()
    {
    }

    public InteractionMessage(string argument)
    {
        var args = argument
            .Split(Separater)
            .Select(x => x.Split('='))
            .Select(x => (x[0], x[1]));

        foreach (var (key, value) in args)
        {
            switch (key)
            {
                case MacroKeyName:
                    MacroKey = value;
                    break;
                case ProcessIdName:
                    ProcessId = value;
                    break;
                default:
                    Add(key, value);
                    break;
            }
        }
    }

    public void Add(string key, string value)
    {
        Arguments.Add(key, value);
    }

    public Dictionary<string, string> ToDictionary()
    {
        return Arguments.ToDictionary(x => x.Key, x => x.Value);
    }

    public string ToArgument()
    {
        var sb = new StringBuilder();
        sb.Append($"macro_key={MacroKey}");
        sb.Append(Separater);
        sb.Append($"process_id={ProcessId}");
        foreach(var (key, value) in Arguments)
        {
            sb.Append(Separater);
            sb.Append($"{key}={value}");
        }
        return sb.ToString();
    }

}