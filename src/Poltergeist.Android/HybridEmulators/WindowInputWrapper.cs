using System.Drawing;
using Microsoft.Extensions.Options;
using Poltergeist.Android.Adb;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Utilities.Windows;
using Poltergeist.Operations.Inputting;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Android.HybridEmulators;

public class WindowInputWrapper : MacroService, IHybridInputService
{
    public MouseSendMessageService MouseSendMessageService { get; }
    private readonly AdbInputOptions AdbDefaultOptions;
    private readonly TimerService TimerService;

    public WindowInputWrapper(
        MacroProcessor processor,
        MouseSendMessageService mouseSendMessageService,
        TimerService timerService,
        IOptions<AdbInputOptions> options) : base(processor)
    {
        MouseSendMessageService = mouseSendMessageService;
        TimerService = timerService;
        AdbDefaultOptions = options.Value;
    }

    public Point Tap(PositionToken position)
    {
        return MouseSendMessageService.Click(position, MouseButtons.Left);
    }

    public Point LongTap(PositionToken position)
    {
        var pointOnWorkspace = MouseSendMessageService.Down(position, MouseButtons.Left);
        var duration = AdbDefaultOptions?.LongPressTime ?? TimeSpanRange.FromMilliseconds(3000, 3000);
        TimerService.Delay(new RangeDelay(duration));
        return MouseSendMessageService.Up(new PrecisePoint(pointOnWorkspace), MouseButtons.Left);
    }

    public Point Swipe(PositionToken beginPosition, PositionToken endPosition)
    {
        var beginPointOnWorkspace = MouseSendMessageService.Down(beginPosition, MouseButtons.Left);
        var endPointOnWorkspace = MouseSendMessageService.Move(new PrecisePoint(beginPointOnWorkspace), endPosition, MouseButtons.Left, options: new MouseInputOptions()
        {
            Motion = AdbDefaultOptions.MovingMotion ?? MouseMoveMotion.Linear,
        });
        return MouseSendMessageService.Up(new PrecisePoint(endPointOnWorkspace), MouseButtons.Left);
    }

    public Point GetTargetPoint(PositionToken position)
    {
        return MouseSendMessageService.GetTargetPoint(position).ToWorkspace;
    }

}
