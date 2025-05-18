using System.Drawing;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Structures.Shapes;

namespace Poltergeist.Android.Emulators;

public class EmulatorEmptyInputService(MacroProcessor processor) : MacroService(processor), IEmulatorInputProvider
{
    public void Drag() => throw new NotSupportedException();
    public void Drop() => throw new NotSupportedException();
    public void LongTap() => throw new NotSupportedException();
    public void MoveTo(Point targetPoint) => throw new NotSupportedException();
    public void MoveTo(Rectangle targetRectangle) => throw new NotSupportedException();
    public void MoveTo(IShape targetShape) => throw new NotSupportedException();
    public void MoveTo(int x, int y) => throw new NotSupportedException();
    public void Tap() => throw new NotSupportedException();
}
