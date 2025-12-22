using System.Drawing;
using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Operations.Inputting;

public class MouseClickedHook : MacroHook
{
    public Point? Location { get; init; }
}
