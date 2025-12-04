using System.Drawing;
using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Android.Adb;

public class AdbTappedHook : MacroHook
{
    public Point Location { get; init; }
}
