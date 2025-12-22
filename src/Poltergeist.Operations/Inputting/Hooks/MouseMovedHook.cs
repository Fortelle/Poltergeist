using System.Drawing;
using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Operations.Inputting;

public class MouseMovedHook : MacroHook
{
    public Point? BeginLocation { get; init; }
    public Point EndLocation { get; init; }
    public Point[]? Path { get; init; }
}
