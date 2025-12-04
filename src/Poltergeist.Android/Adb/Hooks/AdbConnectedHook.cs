using System.Drawing;
using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Android.Adb;

public class AdbConnectedHook : MacroHook
{
    public required string Address { get; init; }
    public required Size ScreenSize { get; init; }
    public Version? AdbVersion { get; init; }
    public Version? AandroidVersion { get; init; }
}
