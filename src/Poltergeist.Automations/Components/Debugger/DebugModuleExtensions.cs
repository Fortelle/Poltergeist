using Poltergeist.Automations.Processors;

namespace Poltergeist.Automations.Components.Debugger;

public static class DebugModuleExtensions
{
    public static bool IsDebugMode(this ArgumentService args)
    {
        return args.Processor.Environments.GetValueOrDefault<bool>(DebugModule.DebugModeKey);
    }
}
