using System.Drawing;
using Microsoft.Extensions.Options;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Structures.Shapes;
using Poltergeist.Automations.Utilities.Maths;
using Poltergeist.Operations.Inputting;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Android.Adb;

// todo: support keyevent/motionevent
public class AdbInputService : MacroService
{
    private readonly AdbService AdbService;
    private readonly AdbLocatingService AdbLocatingService;
    private readonly DeviationService DeviationService;
    private readonly TimerService TimerService;
    private readonly AdbInputOptions DefaultOptions;

    private WCPoint? LastPosition;

    public AdbInputService(MacroProcessor processor,
        AdbService adbService,
        AdbLocatingService adbLocatingService,
        DeviationService deviationService,
        TimerService timerService,
        IOptions<AdbInputOptions> options
        ) : base(processor)
    {
        AdbService = adbService;
        AdbLocatingService = adbLocatingService;
        DeviationService = deviationService;
        TimerService = timerService;
        DefaultOptions = options.Value;
    }

    public Point Tap(PositionToken position, AdbInputOptions? options = null)
    {
        Logger.Trace($"Simulating finger tap action.", new { position, options });
        Logger.IncreaseIndent();

        var targetPoint = GetTargetPoint(position, options);

        AdbService.Shell($"input tap {targetPoint.ToClient.X} {targetPoint.ToClient.Y}");

        LastPosition = targetPoint;

        Processor.GetService<HookService>().Raise(new AdbTappedHook()
        {
            Location = targetPoint.ToWorkspace,
        });

        Logger.Debug($"Simulated a finger tap action at ({targetPoint.ToClient.X},{targetPoint.ToClient.Y}) on the android device.");
        Logger.DecreaseIndent();

        return targetPoint.ToWorkspace;
    }

    public Point LongTap(PositionToken position, AdbInputOptions? options = null)
    {
        Logger.Trace($"Simulating finger long-tap action.", new { position, options });
        Logger.IncreaseIndent();

        var targetPoint = GetTargetPoint(position, options);
        var longPressTime = options?.LongPressTime ?? DefaultOptions?.LongPressTime ?? TimeSpanRange.FromMilliseconds(3000, 3000);
        var duration = TimerService.GetTimeout(new RangeDelay(longPressTime));

        AdbService.Shell($"input swipe {targetPoint.ToClient.X} {targetPoint.ToClient.Y} {targetPoint.ToClient.X} {targetPoint.ToClient.Y} {duration}");

        LastPosition = targetPoint;

        Processor.GetService<HookService>().Raise(new AdbLongTappedHook()
        {
            Location = targetPoint.ToWorkspace,
        });

        Logger.Debug($"Simulated a long tap action at ({targetPoint.ToClient.X},{targetPoint.ToClient.Y}) for {duration}ms on the android device.");
        Logger.DecreaseIndent();

        return targetPoint.ToWorkspace;
    }

    public Point DragAndDrop(PositionToken beginPosition, PositionToken endPosition, AdbInputOptions? options = null)
    {
        if (endPosition is LastPoint && LastPosition is not null)
        {
            return LastPosition.ToWorkspace;
        }

        Logger.Trace($"Simulating drag and drop action.", new { beginPosition, endPosition, options });
        Logger.IncreaseIndent();

        var beginPoint = GetTargetPoint(beginPosition, options);
        var endPoint = GetTargetPoint(endPosition, options);
        var swipeTime = options?.SwipeTime ?? DefaultOptions?.SwipeTime ?? default;
        var duration = TimerService.GetTimeout(new RangeDelay(swipeTime));

        if (duration == 0)
        {
            AdbService.Shell($"input draganddrop {beginPoint.ToClient.X} {beginPoint.ToClient.Y} {endPoint.ToClient.X} {endPoint.ToClient.Y}");
        }
        else
        {
            AdbService.Shell($"input draganddrop {beginPoint.ToClient.X} {beginPoint.ToClient.Y} {endPoint.ToClient.X} {endPoint.ToClient.Y} {duration}");
        }

        LastPosition = endPoint;

        Processor.GetService<HookService>().Raise(new AdbDraggedAndDroppedHook()
        {
            BeginLocation = beginPoint.ToWorkspace,
            EndLocation = endPoint.ToWorkspace,
            Duration = duration,
        });

        Logger.Debug($"Simulated a drag-and-drop action from ({beginPoint.ToClient.X},{beginPoint.ToClient.Y}) to ({endPoint.ToClient.X},{endPoint.ToClient.Y}) for {duration}ms on the android service.");
        Logger.DecreaseIndent();

        return endPoint.ToWorkspace;
    }

    public Point Swipe(PositionToken beginPosition, PositionToken endPosition, AdbInputOptions? options = null)
    {
        if (endPosition is LastPoint && LastPosition is not null)
        {
            return LastPosition.ToWorkspace;
        }

        Logger.Trace($"Simulating finger swipe action.", new { beginPosition, endPosition, options });
        Logger.IncreaseIndent();

        var beginPoint = GetTargetPoint(beginPosition, options);
        var endPoint = GetTargetPoint(endPosition, options);
        var swipeTime = options?.SwipeTime ?? DefaultOptions?.SwipeTime ?? default;
        var duration = TimerService.GetTimeout(new RangeDelay(swipeTime));

        if (duration == 0)
        {
            AdbService.Shell($"input swipe {beginPoint.ToClient.X} {beginPoint.ToClient.Y} {endPoint.ToClient.X} {endPoint.ToClient.Y}");
        }
        else
        {
            AdbService.Shell($"input swipe {beginPoint.ToClient.X} {beginPoint.ToClient.Y} {endPoint.ToClient.X} {endPoint.ToClient.Y} {duration}");
        }

        LastPosition = endPoint;

        Processor.GetService<HookService>().Raise(new AdbSwipedHook()
        {
            BeginLocation = beginPoint.ToWorkspace,
            EndLocation = endPoint.ToWorkspace,
            Duration = duration,
        });

        Logger.Debug($"Simulated a finger swipe action from ({beginPoint.ToClient.X},{beginPoint.ToClient.Y}) to ({endPoint.ToClient.X},{endPoint.ToClient.Y}) for {duration}ms on the android service.");
        Logger.DecreaseIndent();

        return endPoint.ToWorkspace;
    }

    public void Text(string text, AdbInputOptions? options = null)
    {
        Logger.Trace($"Simulating text input action.", new { text, options });
        Logger.IncreaseIndent();

        AdbService.Shell($"input text {text}");

        Logger.Debug($"Input text \"{text}\" to the android service.");
        Logger.DecreaseIndent();

        Processor.GetService<HookService>().Raise(new AdbTextInputHook()
        {
            Text = text,
        });
    }

    public WCPoint GetTargetPoint(PositionToken position, AdbInputOptions? options = null)
    {
        Point pointOnWorkspace;
        switch (position)
        {
            case PrecisePoint precisePoint:
                {
                    pointOnWorkspace = precisePoint.Location;
                }
                break;
            case CoarsePoint coarsePoint:
                {
                    var maxDeviationRadius = coarsePoint.MaxDeviationRadius ?? options?.MaxDeviationRadius ?? DefaultOptions?.MaxDeviationRadius ?? 0;
                    var distribution = coarsePoint.DeviationDistribution ?? options?.DeviationDistribution ?? DefaultOptions?.DeviationDistribution ?? ShapeDistributionType.Uniform;
                    var circle = new CircleShape(coarsePoint.Location, maxDeviationRadius);
                    pointOnWorkspace = DeviationService.GetRandomPoint(circle, new Rectangle(default, AdbLocatingService.GetWorkspaceSize()), distribution);
                }
                break;
            case ShapePosition shapePosition:
                {
                    var shapeDistribution = shapePosition.ShapeDistribution ?? options?.ShapeDistribution ?? DefaultOptions?.ShapeDistribution ?? default;
                    pointOnWorkspace = DeviationService.GetRandomPoint(shapePosition.Shape, new Rectangle(default, AdbLocatingService.GetWorkspaceSize()), shapeDistribution);
                }
                break;
            case LastPoint when LastPosition is not null:
                {
                    return LastPosition;
                }
            default:
                throw new NotImplementedException();
        }
        var pointOnClient = AdbLocatingService.WorkspacePointToClient(pointOnWorkspace);
        return new WCPoint(pointOnWorkspace, pointOnClient);
    }
}
