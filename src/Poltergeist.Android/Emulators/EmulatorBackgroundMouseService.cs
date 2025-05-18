using System.Drawing;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Structures.Shapes;
using Poltergeist.Operations.Background;

namespace Poltergeist.Android.Emulators;

public class EmulatorBackgroundMouseService : MacroService, IEmulatorInputProvider
{
    private Point TargetPoint;

    private readonly BackgroundMouseService Mouse;

    public EmulatorBackgroundMouseService(MacroProcessor processor,
        BackgroundMouseService mouse) : base(processor)
    {
        Mouse = mouse;
    }

    public void MoveTo(Point targetPoint)
    {
        TargetPoint = Mouse.GetPoint(targetPoint);
    }

    public void MoveTo(int x, int y)
    {
        var targetPoint = new Point(x, y);
        TargetPoint = Mouse.GetPoint(targetPoint);
    }

    public void MoveTo(Rectangle targetRectangle)
    {
        TargetPoint = Mouse.GetPoint(targetRectangle);
    }

    public void MoveTo(IShape targetShape)
    {
        TargetPoint = Mouse.GetPoint(targetShape);
    }

    public void Tap()
    {
        Mouse.Click((uint)TargetPoint.X, (uint)TargetPoint.Y);
    }


    public void LongTap() => throw new NotImplementedException();
    public void Drag() => throw new NotImplementedException();
    public void Drop() => throw new NotImplementedException();
}
