using System.Drawing;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Common.Structures.Shapes;
using Poltergeist.Input.Windows;
using Poltergeist.Operations.ForegroundWindows;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.AndroidEmulators;

public class EmulatorMouseService : MacroService, IEmulatorInputSource
{
    public ForegroundMouseService Mouse { get; }
    public TimerService Timer { get; }

    private bool IsDragging { get; set; }

    public EmulatorMouseService(MacroProcessor processor,
        ForegroundMouseService mouse,
        TimerService timer) : base(processor)
    {
        Mouse = mouse;
        Timer = timer;
    }

    public IEmulatorInputSource MoveTo(Point targetPoint)
    {
        Mouse.MoveTo(targetPoint);
        return this;
    }

    public IEmulatorInputSource MoveTo(int x, int y)
    {
        Mouse.MoveTo(x, y);
        return this;
    }

    public IEmulatorInputSource MoveTo(Rectangle targetRectangle)
    {
        Mouse.MoveTo(targetRectangle);
        return this;
    }

    public IEmulatorInputSource MoveTo(IShape targetShape)
    {
        Mouse.MoveTo(targetShape);
        return this;
    }

    public IEmulatorInputSource Tap()
    {
        Mouse.Click();
        return this;
    }

    public IEmulatorInputSource LongTap()
    {
        Mouse.MouseDown(MouseButtons.Left);
        Timer.Delay(3000);
        Mouse.MouseUp(MouseButtons.Left);
        return this;
    }

    public IEmulatorInputSource Drag()
    {
        Mouse.MouseDown(MouseButtons.Left);
        IsDragging = true;
        return this;
    }

    public IEmulatorInputSource Drop()
    {
        Mouse.MouseUp(MouseButtons.Left);
        IsDragging = false;
        return this;
    }

}
