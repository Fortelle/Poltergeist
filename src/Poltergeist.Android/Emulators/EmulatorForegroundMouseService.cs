using System.Drawing;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Structures.Shapes;
using Poltergeist.Automations.Utilities.Windows;
using Poltergeist.Operations.Foreground;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Android.Emulators;

public class EmulatorForegroundMouseService : MacroService, IEmulatorInputProvider
{
    private bool IsDragging;

    private readonly ForegroundMouseService Mouse;
    private readonly TimerService Timer;

    public EmulatorForegroundMouseService(MacroProcessor processor,
        ForegroundMouseService mouse,
        TimerService timer) : base(processor)
    {
        Mouse = mouse;
        Timer = timer;
    }

    public void MoveTo(Point targetPoint)
    {
        Mouse.MoveTo(targetPoint, new MouseInputOptions()
        {
            Motion = IsDragging ? MouseMoveMotion.Linear : null
        });
    }

    public void MoveTo(int x, int y)
    {
        Mouse.MoveTo(x, y, new MouseInputOptions()
        {
            Motion = IsDragging ? MouseMoveMotion.Linear : null
        });
    }

    public void MoveTo(Rectangle targetRectangle)
    {
        Mouse.MoveTo(targetRectangle, new MouseInputOptions()
        {
            Motion = IsDragging ? MouseMoveMotion.Linear : null
        });
    }

    public void MoveTo(IShape targetShape)
    {
        Mouse.MoveTo(targetShape, new MouseInputOptions()
        {
            Motion = IsDragging ? MouseMoveMotion.Linear : null
        });
    }

    public void Tap()
    {
        Mouse.Click();
    }

    public void LongTap()
    {
        Mouse.MouseDown(MouseButtons.Left);
        Timer.Delay(3000);
        Mouse.MouseUp(MouseButtons.Left);
    }

    public void Drag()
    {
        Mouse.MouseDown(MouseButtons.Left);
        IsDragging = true;
    }

    public void Drop()
    {
        Mouse.MouseUp(MouseButtons.Left);
        IsDragging = false;
    }

}
