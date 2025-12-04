using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Android.Adb;

public class AdbClosedHook : MacroHook
{
    public required string Address { get; init; }
}
