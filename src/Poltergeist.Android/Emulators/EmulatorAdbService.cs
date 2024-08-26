using System.Drawing;
using Poltergeist.Android.Adb;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Structures.Shapes;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Android.Emulators;

public class EmulatorAdbService : MacroService, IEmulatorInputProvider
{
    public AdbInputService Adb { get; }
    public TimerService Timer { get; }

    private Point TargetPoint;
    private Point DragPoint;

    public EmulatorAdbService(MacroProcessor processor,
        AdbInputService adb,
        TimerService timer) : base(processor)
    {
        Adb = adb;
        Timer = timer;
    }

    public void MoveTo(Point targetPoint)
    {
        //Logger.Debug($"Moving pseudo cursor to {targetPoint}.");

        TargetPoint = Adb.GetPoint(targetPoint);
    }

    public void MoveTo(int x, int y)
    {
        var targetPoint = new Point(x, y);
        //Logger.Debug($"Moving pseudo cursor to {targetPoint}.");

        TargetPoint = Adb.GetPoint(targetPoint);
    }

    public void MoveTo(Rectangle targetRectangle)
    {
        //Logger.Debug($"Moving pseudo cursor to {targetRectangle}.");

        TargetPoint = Adb.GetPoint(targetRectangle);
    }

    public void MoveTo(IShape targetShape)
    {
        //Logger.Debug($"Moving pseudo cursor to {targetShape}.");

        TargetPoint = Adb.GetPoint(targetShape);
    }

    public void Tap()
    {
        //Logger.Debug($"Simulating tap.");

        Adb.DoTap(TargetPoint);
    }

    public void LongTap()
    {
        //Logger.Debug($"Simulating long-tap.");

        Adb.DoLongTap(TargetPoint);
    }

    public void Drag()
    {
        //Logger.Debug($"Simulating drag and drop.");

        DragPoint = TargetPoint;
    }

    public void Drop()
    {
        Adb.Swipe(DragPoint, TargetPoint);
    }

}
