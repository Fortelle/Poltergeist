using System.Drawing;
using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Operations.Inputing;

public class MouseWheeledHook : MacroHook
{
    public Point? Location { get; init; }
    public MouseWheelDirection Direction { get; init; }
    public int Detents { get; init; }
}
