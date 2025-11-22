using System.Drawing;
using Microsoft.Extensions.Options;
using Poltergeist.Android.Adb;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Utilities.Windows;
using Poltergeist.Operations.Inputing;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Android.HybridEmulators;

public class ScreenInputWrapper : MacroService, IHybridInputService
{
    public MouseSendInputService MouseSendInputService { get; }
    private readonly AdbInputOptions AdbDefaultOptions;
    private readonly TimerService TimerService;

    public ScreenInputWrapper(
        MacroProcessor processor,
        MouseSendInputService mouseSendInputService,
        TimerService timerService,
        IOptions<AdbInputOptions> options) : base(processor)
    {
        MouseSendInputService = mouseSendInputService;
        TimerService = timerService;
        AdbDefaultOptions = options.Value;
    }

    public Point Tap(PositionToken position)
    {
        var pointOnWorkspace = MouseSendInputService.MoveTo(position);
        MouseSendInputService.Click(MouseButtons.Left);
        return pointOnWorkspace;
    }

    public Point LongTap(PositionToken position)
    {
        var pointOnWorkspace = MouseSendInputService.MoveTo(position);
        TimerService.GetTimeout(AdbDefaultOptions?.LongPressTime ?? TimeSpanRange.FromMilliseconds(3000, 3000));
        MouseSendInputService.Click(MouseButtons.Left);
        return pointOnWorkspace;
    }

    public Point Swipe(PositionToken beginPosition, PositionToken endPosition)
    {
        var beginPointOnWorkspace = MouseSendInputService.MoveTo(beginPosition);
        MouseSendInputService.Down(MouseButtons.Left);
        var endPointOnWorkspace = MouseSendInputService.Move(new PrecisePoint(beginPointOnWorkspace), endPosition, new MouseInputOptions()
        {
            Motion = AdbDefaultOptions.MovingMotion ?? MouseMoveMotion.Linear,
        });
        MouseSendInputService.Up(MouseButtons.Left);
        return endPointOnWorkspace;
    }

    public Point GetTargetPoint(PositionToken position)
    {
        return MouseSendInputService.GetTargetPoint(position).ToWorkspace;
    }
}
