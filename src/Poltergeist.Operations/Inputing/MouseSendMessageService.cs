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

namespace Poltergeist.Operations.Inputing;

public class MouseSendMessageService : MacroService
{
    private readonly WindowLocatingService WindowLocatingService;
    private readonly TimerService TimerService;
    private readonly DeviationService DeviationService;
    private readonly MouseInputOptions DefaultOptions;

    private WCPoint? LastPosition;

    public MouseSendMessageService(
        MacroProcessor processor,
        WindowLocatingService windowLocatingService,
        TimerService timerService,
        DeviationService deviationService,
        IOptions<MouseInputOptions> options
        ) : base(processor)
    {
        WindowLocatingService = windowLocatingService;
        TimerService = timerService;
        DeviationService = deviationService;
        DefaultOptions = options.Value;
    }

    public Point Click(PositionToken position, MouseButtons button, KeyModifiers modifier = KeyModifiers.None, MouseInputOptions? options = null)
    {
        Logger.Trace($"Simulating mouse click action.", new { position, button, modifier, options });
        Logger.IncreaseIndent();

        var targetPoint = GetTargetPoint(position, options);
        var downUpInterval = options?.MouseDownUpInterval ?? DefaultOptions?.MouseDownUpInterval ?? default;

        SendMouseDownMessage(targetPoint.ToClient, button, modifier);
        Delay(downUpInterval);
        SendMouseUpMessage(targetPoint.ToClient, button, modifier);

        LastPosition = targetPoint;

        Processor.GetService<HookService>().Raise(new MouseClickedHook()
        {
            Location = targetPoint.ToWorkspace,
        });

        Logger.Debug($"Simulated a mouse click action at ({targetPoint.ToClient.X},{targetPoint.ToClient.Y}) with the {button} button on the client window.");
        Logger.DecreaseIndent();

        return targetPoint.ToWorkspace;
    }

    public Point DoubleClick(PositionToken position, MouseButtons button, KeyModifiers modifier = KeyModifiers.None, MouseInputOptions? options = null)
    {
        Logger.Trace($"Simulating mouse double-click action.", new { position, button, modifier, options });
        Logger.IncreaseIndent();

        var targetPoint = GetTargetPoint(position, options);
        var downUpInterval = options?.MouseDownUpInterval ?? DefaultOptions?.MouseDownUpInterval ?? default;
        var doubleClickInterval = options?.DoubleClickInterval ?? DefaultOptions?.DoubleClickInterval ?? downUpInterval;

        SendMouseDownMessage(targetPoint.ToClient, button, modifier);
        Delay(downUpInterval);
        SendMouseUpMessage(targetPoint.ToClient, button, modifier);
        Delay(doubleClickInterval);
        SendMouseDoubleClickMessage(targetPoint.ToClient, button, modifier);
        Delay(downUpInterval);
        SendMouseUpMessage(targetPoint.ToClient, button, modifier);

        LastPosition = targetPoint;

        Processor.GetService<HookService>().Raise(new MouseDoubleClickedHook()
        {
            Location = targetPoint.ToWorkspace,
        });

        Logger.Debug($"Simulated a mouse double-click action at ({targetPoint.ToClient.X},{targetPoint.ToClient.Y}) with the {button} button on the client window.");
        Logger.DecreaseIndent();

        return targetPoint.ToWorkspace;
    }

    public Point Down(PositionToken position, MouseButtons button, KeyModifiers modifier = KeyModifiers.None, MouseInputOptions? options = null)
    {
        Logger.Trace($"Simulating mouse down action.", new { position, button, modifier, options });
        Logger.IncreaseIndent();

        var targetPoint = GetTargetPoint(position, options);

        SendMouseDownMessage(targetPoint.ToClient, button, modifier);

        LastPosition = targetPoint;

        Processor.GetService<HookService>().Raise(new MouseDownHook()
        {
            Location = targetPoint.ToWorkspace,
        });

        Logger.Debug($"Simulated a mouse down action at ({targetPoint.ToClient.X},{targetPoint.ToClient.Y}) with the {button} button on the client window.");
        Logger.DecreaseIndent();

        return targetPoint.ToWorkspace;
    }

    public Point Up(PositionToken position, MouseButtons button, KeyModifiers modifier = KeyModifiers.None, MouseInputOptions? options = null)
    {
        Logger.Trace($"Simulating mouse up action.", new { position, button, modifier, options });
        Logger.IncreaseIndent();

        var targetPoint = GetTargetPoint(position, options);

        SendMouseDownMessage(targetPoint.ToClient, button, modifier);

        LastPosition = targetPoint;

        Processor.GetService<HookService>().Raise(new MouseUpHook()
        {
            Location = targetPoint.ToWorkspace,
        });

        Logger.Debug($"Simulated a mouse up action at ({targetPoint.ToClient.X},{targetPoint.ToClient.Y}) with the {button} button on the client window.");
        Logger.DecreaseIndent();

        return targetPoint.ToWorkspace;
    }

    public Point Move(PositionToken beginPosition, PositionToken endPosition, MouseButtons button, KeyModifiers modifier = KeyModifiers.None, MouseInputOptions? options = null)
    {
        if (endPosition is LastPoint && LastPosition is not null)
        {
            return LastPosition.ToWorkspace;
        }

        Logger.Trace($"Simulating mouse move action.", new { beginPosition, endPosition, button, modifier, options });
        Logger.IncreaseIndent();

        var endPoint = GetTargetPoint(endPosition, options);
        var motion = options?.Motion ?? DefaultOptions?.Motion ?? MouseMoveMotion.Jump;

        if (motion == MouseMoveMotion.Jump)
        {
            SendMouseMoveMessage(endPoint.ToClient, button, modifier);

            Processor.GetService<HookService>().Raise(new MouseMovedHook()
            {
                EndLocation = endPoint.ToWorkspace,
            });

            Logger.Debug($"Simulated a mouse move action at ({endPoint.ToClient.X},{endPoint.ToClient.Y}) with the {button} button on the client window.");
        }
        else
        {
            var beginPoint = GetTargetPoint(beginPosition, options);
            var path = motion switch
            {
                MouseMoveMotion.Linear => CursorHelper.GetLinearPositions(beginPoint.ToClient, endPoint.ToClient),
                _ => throw new NotImplementedException(),
            };
            var interval = MouseInputOptions.MouseMoveInterval;

            foreach (var point in path)
            {
                SendMouseMoveMessage(point, button, modifier);
                Delay(interval);
            }

            Processor.GetService<HookService>().Raise(new MouseMovedHook()
            {
                BeginLocation = beginPoint.ToWorkspace,
                EndLocation = endPoint.ToWorkspace,
                Path = path,
            });

            Logger.Debug($"Simulated a mouse move action from ({beginPoint.ToClient.X},{beginPoint.ToClient.Y}) to ({endPoint.ToClient.X},{endPoint.ToClient.Y}) with the {button} button on the client window.");
        }

        LastPosition = endPoint;

        Logger.DecreaseIndent();

        return endPoint.ToClient;
    }

    public Point MoveTo(PositionToken endPosition, MouseButtons button, KeyModifiers modifier = KeyModifiers.None, MouseInputOptions? options = null)
    {
        return Move(new LastPoint(), endPosition, button, modifier, options);
    }

    public void Wheel(PositionToken position, MouseWheelDirection direction, int detents, MouseButtons button = MouseButtons.None, KeyModifiers modifier = KeyModifiers.None, MouseInputOptions? options = null)
    {
        Logger.Trace($"Simulating mouse wheel action.", new { position, direction, detents, button, modifier, options });
        Logger.IncreaseIndent();

        var targetPoint = GetTargetPoint(position, options);

        switch (direction)
        {
            case MouseWheelDirection.Forward:
                SendMouseWheelMessage(targetPoint.ToClient, detents, button, modifier);
                break;
            case MouseWheelDirection.Backward:
                SendMouseWheelMessage(targetPoint.ToClient, -detents, button, modifier);
                break;
            case MouseWheelDirection.Left:
                SendMouseHWheelMessage(targetPoint.ToClient, -detents, button, modifier);
                break;
            case MouseWheelDirection.Right:
                SendMouseHWheelMessage(targetPoint.ToClient, detents, button, modifier);
                break;
            default:
                throw new NotSupportedException();
        }

        LastPosition = targetPoint;

        Processor.GetService<HookService>().Raise(new MouseWheeledHook()
        {
            Location = targetPoint.ToWorkspace,
            Direction = direction,
            Detents = detents,
        });

        Logger.Debug($"Simulated a mouse wheel action towards {direction} with the {button} button and {detents} detents on the client window.");
        Logger.DecreaseIndent();
    }

    public WCPoint GetTargetPoint(PositionToken position, MouseInputOptions? options = null)
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
                    pointOnWorkspace = DeviationService.GetRandomPoint(circle, new Rectangle(default, WindowLocatingService.GetWorkspaceSize()), distribution, lastPosition);
                }
                break;
            case ShapePosition shapePosition:
                {
                    var shapeDistribution = shapePosition.ShapeDistribution ?? options?.ShapeDistribution ?? DefaultOptions?.ShapeDistribution ?? default;
                    var keepUnmovedInShape = shapePosition.KeepUnmovedInShape ?? options?.KeepUnmovedInShape ?? DefaultOptions?.KeepUnmovedInShape ?? false;
                    var lastPosition = keepUnmovedInShape ? LastPosition?.ToWorkspace ?? null : null;
                    pointOnWorkspace = DeviationService.GetRandomPoint(shapePosition.Shape, new Rectangle(default, WindowLocatingService.GetWorkspaceSize()), shapeDistribution, lastPosition);
                }
                break;
            case LastPoint when LastPosition is not null:
                {
                    return LastPosition;
                }
            default:
                throw new NotImplementedException();
        }
        var pointOnClient = WindowLocatingService.WorkspacePointToClient(pointOnWorkspace);
        return new WCPoint(pointOnWorkspace, pointOnClient);
    }

    private void SendMouseDownMessage(Point pointOnClient, MouseButtons button, KeyModifiers modifier)
    {
        new SendMessageHelper(WindowLocatingService.Handle, Logger)
            .MouseButtonDown(pointOnClient.X, pointOnClient.Y, button, modifier);
    }

    private void SendMouseUpMessage(Point pointOnClient, MouseButtons button, KeyModifiers modifier )
    {
        new SendMessageHelper(WindowLocatingService.Handle, Logger)
            .MouseButtonUp(pointOnClient.X, pointOnClient.Y, button, modifier);
    }

    private void SendMouseDoubleClickMessage(Point pointOnClient, MouseButtons button, KeyModifiers modifier)
    {
        new SendMessageHelper(WindowLocatingService.Handle, Logger)
            .MouseDoubleClick(pointOnClient.X, pointOnClient.Y, button, modifier);
    }

    private void SendMouseMoveMessage(Point pointOnClient, MouseButtons button, KeyModifiers modifier)
    {
        new SendMessageHelper(WindowLocatingService.Handle, Logger)
            .MouseMove(pointOnClient.X, pointOnClient.Y, button, modifier);
    }

    private void SendMouseWheelMessage(Point pointOnClient, int detents, MouseButtons button, KeyModifiers modifier)
    {
        new SendMessageHelper(WindowLocatingService.Handle, Logger)
            .MouseWheel(pointOnClient.X, pointOnClient.Y, detents, button, modifier);
    }

    private void SendMouseHWheelMessage(Point pointOnClient, int detents, MouseButtons button, KeyModifiers modifier)
    {
        new SendMessageHelper(WindowLocatingService.Handle, Logger)
            .MouseHWheel(pointOnClient.X, pointOnClient.Y, detents, button, modifier);
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

    private void Delay(TimeSpanRange range) => Delay(TimerService.GetTimeout(range));

}
