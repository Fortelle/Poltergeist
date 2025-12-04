using System.Drawing;
using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Operations.Inputing;

public class MouseDownHook : MacroHook
{
    public Point? Location { get; init; }
}
