using System.Drawing;
using Microsoft.Extensions.Options;
using Poltergeist.Automations.Components.Hooks;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Structures.Shapes;
using Poltergeist.Automations.Utilities.Maths;
using Poltergeist.Automations.Utilities.Windows;
using Poltergeist.Operations.Locating;
using Poltergeist.Operations.Timers;

namespace Poltergeist.Operations.Inputting;

public class MouseSendInputService : MacroService
{
    private readonly ScreenLocatingService ScreenLocatingService;
    private readonly DeviationService DeviationService;
    private readonly TimerService TimerService;
    private readonly MouseInputOptions DefaultOptions;

    private WCSPoint? LastPosition;

    public MouseSendInputService(
        MacroProcessor processor,
        ScreenLocatingService screenLocatingService,
        TimerService timerService,
        DeviationService deviationService,
        IOptions<MouseInputOptions> options
        ) : base(processor)
    {
        ScreenLocatingService = screenLocatingService;
        DeviationService = deviationService;
        TimerService = timerService;
        DefaultOptions = options.Value;
    }

    public void Click(MouseButtons button, MouseInputOptions? options = null)
    {
        Logger.Trace($"Simulating mouse click action.", new { button, options });
        Logger.IncreaseIndent();

        var interval = options?.MouseDownUpInterval ?? DefaultOptions?.MouseDownUpInterval ?? default;

        if (interval == default)
        {
            SendClickMessage(button);
        }
        else
        {
            SendMouseDownMessage(button);
            Delay(interval);
            SendMouseUpMessage(button);
        }

        Processor.GetService<HookService>().Raise(new MouseClickedHook());

        Logger.Debug($"Simulated a mouse click action with the {button} button.");
        Logger.DecreaseIndent();
    }

    public void DoubleClick(MouseButtons button, MouseInputOptions? options = null)
    {
        Logger.Trace($"Simulating mouse double-click action.", new { button, options });
        Logger.IncreaseIndent();

        var downUpInterval = options?.MouseDownUpInterval ?? DefaultOptions?.MouseDownUpInterval ?? default;
        var doubleClickInterval = options?.DoubleClickInterval ?? DefaultOptions?.DoubleClickInterval ?? downUpInterval;

        if (downUpInterval == default && doubleClickInterval == default)
        {
            SendDoubleClickMessage(button);
        }
        else
        {
            SendMouseDownMessage(button);
            Delay(downUpInterval);
            SendMouseUpMessage(button);
            Delay(doubleClickInterval);
            SendMouseDownMessage(button);
            Delay(downUpInterval);
            SendMouseUpMessage(button);
        }

        Processor.GetService<HookService>().Raise(new MouseDoubleClickedHook());

        Logger.Debug($"Simulated a mouse double-clicked action with the {button} button.");
        Logger.DecreaseIndent();
    }

    public void Down(MouseButtons button, MouseInputOptions? options = null)
    {
        Logger.Trace($"Simulating mouse down action.", new { button });
        Logger.IncreaseIndent();

        SendMouseDownMessage(button);

        Processor.GetService<HookService>().Raise(new MouseDownHook());

        Logger.Debug($"Simulated a mouse down action with the {button} button.");
        Logger.DecreaseIndent();
    }

    public void Up(MouseButtons button, MouseInputOptions? options = null)
    {
        Logger.Trace($"Simulating mouse up action.", new { button });
        Logger.IncreaseIndent();

        SendMouseUpMessage(button);

        Processor.GetService<HookService>().Raise(new MouseUpHook());

        Logger.Debug($"Simulated a mouse up action with the {button} button.");
        Logger.DecreaseIndent();
    }

    public void Wheel(MouseWheelDirection direction, int detents, MouseInputOptions? options = null)
    {
        Logger.Trace($"Simulating mouse wheel action.", new { direction, detents, options });
        Logger.IncreaseIndent();

        switch (direction)
        {
            case MouseWheelDirection.Forward:
                DoVerticalWheel(detents, options);
                break;
            case MouseWheelDirection.Backward:
                DoVerticalWheel(-detents, options);
                break;
            case MouseWheelDirection.Left:
                DoHorizontalWheel(-detents, options);
                break;
            case MouseWheelDirection.Right:
                DoHorizontalWheel(detents, options);
                break;
            default:
                throw new NotSupportedException();
        }

        Processor.GetService<HookService>().Raise(new MouseWheeledHook()
        {
            Direction = direction,
            Detents = detents
        });

        Logger.Debug($"Simulated a mouse wheel action towards {direction} with {detents} detents.");
        Logger.DecreaseIndent();
    }

    public Point Move(PositionToken beginPosition, PositionToken endPosition, MouseInputOptions? options = null)
    {
        if (endPosition is LastPoint && LastPosition is not null)
        {
            return LastPosition.ToWorkspace;
        }

        Logger.Trace($"Simulating mouse move.", new { beginPosition, endPosition, options });
        Logger.IncreaseIndent();

        var endPoint = GetTargetPoint(endPosition, options);
        var motion = options?.Motion ?? DefaultOptions?.Motion ?? MouseMoveMotion.Jump;

        if (motion == MouseMoveMotion.Jump)
        {
            MoveCursorTo(endPoint.ToScreen);

            Processor.GetService<HookService>().Raise(new MouseMovedHook()
            {
                EndLocation = endPoint.ToWorkspace,
            });

            Logger.Debug($"Simulated a mouse move action at ({endPoint.ToScreen.X},{endPoint.ToScreen.Y}) on the screen.");
        }
        else
        {
            var beginPoint = GetTargetPoint(beginPosition, options);
            var path = motion switch
            {
                MouseMoveMotion.Linear => CursorHelper.GetLinearPositions(beginPoint.ToScreen, endPoint.ToScreen),
                _ => throw new NotImplementedException(),
            };
            var interval = MouseInputOptions.MouseMoveInterval;

            foreach (var point in path)
            {
                MoveCursorTo(point);
                Delay(interval);
            }

            Processor.GetService<HookService>().Raise(new MouseMovedHook()
            {
                BeginLocation = beginPoint.ToWorkspace,
                EndLocation = endPoint.ToWorkspace,
                Path = path,
            });

            Logger.Debug($"Simulated a mouse move action from ({beginPoint.ToScreen.X},{beginPoint.ToScreen.Y}) to ({endPoint.ToScreen.X},{endPoint.ToScreen.Y}) on the screen.");
        }

        LastPosition = endPoint;

        Logger.DecreaseIndent();

        return endPoint.ToWorkspace;
    }

    public Point MoveTo(PositionToken endPosition, MouseInputOptions? options = null)
    {
        return Move(new LastPoint(), endPosition, options);
    }

    public WCSPoint GetTargetPoint(PositionToken position, MouseInputOptions? options = null)
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
                    var keepUnmovedInShape = coarsePoint.KeepUnmovedInShape ?? options?.KeepUnmovedInShape ?? DefaultOptions?.KeepUnmovedInShape ?? false;
                    var lastPosition = keepUnmovedInShape ? LastPosition?.ToWorkspace ?? null : null;
                    var circle = new CircleShape(coarsePoint.Location, maxDeviationRadius);
                    pointOnWorkspace = DeviationService.GetRandomPoint(circle, new Rectangle(default, ScreenLocatingService.GetWorkspaceSize()), distribution, lastPosition);
                }
                break;
            case ShapePosition shapePosition:
                {
                    var shapeDistribution = shapePosition.ShapeDistribution ?? options?.ShapeDistribution ?? DefaultOptions?.ShapeDistribution ?? default;
                    var keepUnmovedInShape = shapePosition.KeepUnmovedInShape ?? options?.KeepUnmovedInShape ?? DefaultOptions?.KeepUnmovedInShape ?? false;
                    var lastPosition = keepUnmovedInShape ? LastPosition?.ToWorkspace ?? null : null;
                    pointOnWorkspace = DeviationService.GetRandomPoint(shapePosition.Shape, new Rectangle(default, ScreenLocatingService.GetWorkspaceSize()), shapeDistribution, lastPosition);
                }
                break;
            case LastPoint when LastPosition is not null:
                {
                    return LastPosition;
                }
            case LastPoint:
                {
                    pointOnWorkspace = ScreenLocatingService.ScreenPointToWorkspace(SendInputHelper.Cursor);
                }
                break;
            default:
                throw new NotImplementedException();
        }
        var pointOnClient = ScreenLocatingService.WorkspacePointToClient(pointOnWorkspace);
        var pointOnScreen = ScreenLocatingService.ClientPointToScreen(pointOnClient);
        return new WCSPoint(pointOnWorkspace, pointOnClient, pointOnScreen);
    }

    private void DoVerticalWheel(int detents, MouseInputOptions? options = null)
    {
        var interval = options?.VerticalWheelInterval ?? DefaultOptions?.VerticalWheelInterval ?? default;

        if (interval == default)
        {
            SendMouseWheelMessage(detents);
        }
        else
        {
            var singleDetent = Math.Sign(detents);
            var absoluteDetent = Math.Abs(detents);
            for (var i = 0; i < absoluteDetent; i++)
            {
                SendMouseWheelMessage(singleDetent);
                if (i < absoluteDetent - 1)
                {
                    Delay(interval);
                }
            }
        }
    }

    private void DoHorizontalWheel(int detents, MouseInputOptions? options = null)
    {
        var interval = options?.HorizontalWheelInterval ?? DefaultOptions?.HorizontalWheelInterval ?? default;

        if (interval == default)
        {
            SendMouseHWheelMessage(detents);
        }
        else
        {
            var singleDetent = Math.Sign(detents);
            var absoluteDetent = Math.Abs(detents);
            for (var i = 0; i < absoluteDetent; i++)
            {
                SendMouseHWheelMessage(singleDetent);
                if (i < absoluteDetent - 1)
                {
                    Delay(interval);
                }
            }
        }
    }


    private void SendMouseDownMessage(MouseButtons button)
    {
        new SendInputHelper()
            .AddMouseDown(button)
            .Execute();

        Logger.Trace($"Sent mouse down input.", new { button });
    }

    private void SendMouseUpMessage(MouseButtons button)
    {
        new SendInputHelper()
            .AddMouseUp(button)
            .Execute();

        Logger.Trace($"Sent mouse up input.", new { button });
    }

    private void SendClickMessage(MouseButtons button)
    {
        new SendInputHelper()
            .AddMouseDown(button)
            .AddMouseUp(button)
            .Execute();

        Logger.Trace($"Sent mouse down-up input.", new { button });
    }

    private void SendDoubleClickMessage(MouseButtons button)
    {
        new SendInputHelper()
            .AddMouseDown(button)
            .AddMouseUp(button)
            .AddMouseDown(button)
            .AddMouseUp(button)
            .Execute();

        Logger.Trace($"Sent mouse down-up-down-up input.", new { button });
    }

    private void SendMouseWheelMessage(int detents)
    {
        var movement = detents * MouseInputOptions.MouseWheelDelta;
        new SendInputHelper().AddMouseWheel(movement).Execute();

        Logger.Trace($"Simulated mouse wheel input.", new { detents, movement });
    }

    private void SendMouseHWheelMessage(int detents)
    {
        var movement = detents * MouseInputOptions.MouseWheelDelta;
        new SendInputHelper().AddMouseHWheel(movement).Execute();

        Logger.Trace($"Simulated mouse hwheel input.", new { detents, movement });
    }

    private void MoveCursorTo(Point pointOnScreen)
    {
        SendInputHelper.Cursor = pointOnScreen;

        Logger.Trace($"SetCursorPos({pointOnScreen.X}, {pointOnScreen.Y})");
    }

    private void Delay(int timeout)
    {
        if (timeout == 0)
        {
            return;
        }
        Thread.Sleep(timeout);
        Logger.Trace($"Delayed for {timeout}ms.");
    }

    private void Delay(TimeSpanRange range) => Delay(TimerService.GetTimeout(new RangeDelay(range)));
}
