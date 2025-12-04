using System.Drawing;
using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Android.Adb;

public class AdbDraggedAndDroppedHook : MacroHook
{
    public Point BeginLocation { get; init; }
    public Point EndLocation { get; init; }
    public int Duration { get; init; }
}
