using System.Drawing;
using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Android.Adb;

public class AdbLongTappedHook : MacroHook
{
    public Point Location { get; init; }
}
