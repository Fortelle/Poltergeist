using System.Drawing;
using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Operations.Inputting;

public class MouseUpHook : MacroHook
{
    public Point? Location { get; init; }
}
