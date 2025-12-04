using System.Drawing;
using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Operations.Capturing;

public class ClientCapturedHook : MacroHook
{
    public Size? ClientSize { get; init; }

    public Bitmap? FullImage { get; init; }

    public Bitmap[]? ClipImages { get; init; }

    public Rectangle[]? ClipAreas { get; init; }

    public Rectangle[]? TargetAreas { get; init; }
}
