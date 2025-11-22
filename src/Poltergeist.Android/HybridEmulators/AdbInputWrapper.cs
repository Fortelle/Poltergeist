using System.Drawing;
using Poltergeist.Android.Adb;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Operations.Inputing;

namespace Poltergeist.Android.HybridEmulators;

public class AdbInputWrapper : MacroService, IHybridInputService
{
    public AdbInputService AdbInputService { get; }

    public AdbInputWrapper(MacroProcessor processor, AdbInputService adbInputService) : base(processor)
    {
        AdbInputService = adbInputService;
    }

    public Point Tap(PositionToken position)
    {
        return AdbInputService.Tap(position);
    }

    public Point LongTap(PositionToken position)
    {
        return AdbInputService.LongTap(position);
    }

    public Point Swipe(PositionToken beginPosition, PositionToken endPosition)
    {
        return AdbInputService.Swipe(beginPosition, endPosition);
    }

    public Point GetTargetPoint(PositionToken position)
    {
        return AdbInputService.GetTargetPoint(position).ToWorkspace;
    }
}
