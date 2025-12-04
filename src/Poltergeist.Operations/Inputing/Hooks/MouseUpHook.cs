using System.Drawing;
using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Operations.Inputing;

public class MouseUpHook : MacroHook
{
    public Point? Location { get; init; }
}
