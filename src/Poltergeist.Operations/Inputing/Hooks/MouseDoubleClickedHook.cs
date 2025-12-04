using System.Drawing;
using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Operations.Inputing;

public class MouseDoubleClickedHook : MacroHook
{
    public Point? Location { get; init; }
}
