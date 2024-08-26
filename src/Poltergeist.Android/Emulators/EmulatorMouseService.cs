using System.Drawing;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Operations.Foreground;
using Poltergeist.Operations.Timers;
using Poltergeist.Automations.Structures.Shapes;
using Poltergeist.Automations.Utilities.Windows;

namespace Poltergeist.Android.Emulators;

public class EmulatorMouseService : MacroService, IEmulatorInputProvider
{
    public ForegroundMouseService Mouse { get; }
    public TimerService Timer { get; }

    private bool IsDragging;

    public EmulatorMouseService(MacroProcessor processor,
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
