using System.Drawing;
using Poltergeist.Automations.Components.Hooks;

namespace Poltergeist.Operations.Capturing;

public class ImageCapturedHook(Bitmap image) : MacroHook
{
    public Bitmap Image => image;
}
