using System.Drawing;
using Poltergeist.Automations.Logging;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Common.Structures.Shapes;
using Poltergeist.Operations.Android;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.AndroidEmulators;

public class EmulatorAdbService : MacroService, IEmulatorInputSource
{
    public AdbInputService Adb { get; }
    public TimerService Timer { get; }

    private Point TargetPoint { get; set; }
    private Point DragPoint { get; set; }

    public EmulatorAdbService(MacroProcessor processor,
        AdbInputService adb,
        TimerService timer) : base(processor)
    {
        Adb = adb;
        Timer = timer;
    }

    public IEmulatorInputSource MoveTo(Point targetPoint)
    {
        //Logger.Debug($"Moving pseudo cursor to {targetPoint}.");

        TargetPoint = Adb.GetPoint(targetPoint);
        return this;
    }

    public IEmulatorInputSource MoveTo(int x, int y)
    {
        var targetPoint = new Point(x, y);
        //Logger.Debug($"Moving pseudo cursor to {targetPoint}.");

        TargetPoint = Adb.GetPoint(targetPoint);
        return this;
    }

    public IEmulatorInputSource MoveTo(Rectangle targetRectangle)
    {
        //Logger.Debug($"Moving pseudo cursor to {targetRectangle}.");

        TargetPoint = Adb.GetPoint(targetRectangle);
        return this;
    }

    public IEmulatorInputSource MoveTo(IShape targetShape)
    {
        //Logger.Debug($"Moving pseudo cursor to {targetShape}.");

        TargetPoint = Adb.GetPoint(targetShape);
        return this;
    }

    public IEmulatorInputSource Tap()
    {
        //Logger.Debug($"Simulating tap.");

        Adb.DoTap(TargetPoint);
        return this;
    }

    public IEmulatorInputSource LongTap()
    {
        //Logger.Debug($"Simulating long-tap.");

        Adb.DoLongTap(TargetPoint);
        return this;
    }

    public IEmulatorInputSource Drag()
    {
        //Logger.Debug($"Simulating drag and drop.");

        DragPoint = TargetPoint;
        return this;
    }

    public IEmulatorInputSource Drop()
    {
        Adb.Swipe(DragPoint, TargetPoint);
        return this;
    }

}
