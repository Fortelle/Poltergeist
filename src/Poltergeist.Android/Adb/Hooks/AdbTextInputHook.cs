using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Android.Adb;

public class AdbTextInputHook : MacroHook
{
    public string Text { get; init; }
}
