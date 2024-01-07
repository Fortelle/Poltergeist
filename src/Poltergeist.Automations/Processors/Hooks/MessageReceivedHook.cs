using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Automations.Processors;

public class MessageReceivedHook : MacroHook
{
    public Dictionary<string, string> Arguments { get; set; }

    public MessageReceivedHook(Dictionary<string, string> arguments)
    {
        Arguments = arguments;
    }
}
