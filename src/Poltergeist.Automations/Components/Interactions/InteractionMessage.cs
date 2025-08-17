using System.Text;

namespace Poltergeist.Automations.Components.Interactions;

public class InteractionMessage
{
    public const string ProcessorIdName = "processor_id";
    private const char Separater = ';';

    public string? ProcessorId { get; set; }

    private readonly Dictionary<string, string> Arguments = new();

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
                case ProcessorIdName:
                    ProcessorId = value;
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
        sb.Append(Separater);
        sb.Append($"{ProcessorIdName}={ProcessorId}");
        foreach(var (key, value) in Arguments)
        {
            sb.Append(Separater);
            sb.Append($"{key}={value}");
        }
        return sb.ToString();
    }

}