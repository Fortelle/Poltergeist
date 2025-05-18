using System.Drawing;
using Microsoft.Extensions.Options;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Structures.Shapes;
using Poltergeist.Automations.Utilities.Maths;
using Poltergeist.Automations.Utilities.Windows;

namespace Poltergeist.Operations.Foreground;

public class ForegroundMouseService : MacroService
{
    private readonly ForegroundLocatingService Locating;
    private readonly DistributionService Distribution;
    private readonly MouseInputOptions DefaultOptions;

    public ForegroundMouseService(
        MacroProcessor processor,
        ForegroundLocatingService locating,
        IOptions<MouseInputOptions> options,
        DistributionService distribution
        ) : base(processor)
    {
        Locating = locating;
        Distribution = distribution;
        DefaultOptions = options.Value;
    }

    #region "click"

    public void Click(MouseButtons button, MouseInputOptions? options = null)
    {
        DoClick(button, options);

        Logger.Debug($"Simulated a mouse {button} click action.");
    }

    public void Click()
    {
        Click(MouseButtons.Left);
    }

    //public void Click(Rectangle targetRectangle, MouseInputOptions? options = null)
    //{
    //    MoveTo(targetRectangle, options);
    //    DoClick(MouseButtons.Left, options);
    //}

    //public void Click(IShape targetShape, MouseInputOptions? options = null)
    //{
    //    MoveTo(targetShape, options);
    //    DoClick(MouseButtons.Left, options);
    //}

    //public void Click(Point targetPoint, MouseInputOptions? options = null)
    //{
    //    MoveTo(targetPoint, options);
    //    DoClick(MouseButtons.Left, options);
    //}

    #endregion


    #region "double click"

    public void DoubleClick(MouseButtons button, MouseInputOptions? options = null)
    {
        DoDoubleClick(button, options);

        Logger.Debug($"Simulated a mouse {button} double-click action.");
    }

    public void DoubleClick()
    {
        DoubleClick(MouseButtons.Left);
    }

    #endregion


    //todo: LineTo
    #region "move"

    public Point MoveBy(int ox, int oy, MouseInputOptions? options = null)
    {
        var clientPoint = GetClientCursorPosition();
        clientPoint.Offset(ox, oy);
        var result = DoMoveTo(clientPoint, options);

        Logger.Debug($"Moved the cursor by offset {{{clientPoint.X},{clientPoint.Y}}}.");

        return result;
    }

    public Point MoveTo(Point clientPoint, MouseInputOptions? options = null)
    {
        var result = DoMoveTo(clientPoint, options);

        Logger.Debug($"Moved the cursor to the client position {{{clientPoint.X},{clientPoint.Y}}}.");

        return result;
    }

    public Point MoveTo(int x, int y, MouseInputOptions? options = null)
    {
        var result = DoMoveTo(new Point(x, y), options);

        Logger.Debug($"Moved the cursor to the client position {{{x},{y}}}.");

        return result;
    }

    public Point MoveTo(IShape targetShape, MouseInputOptions? options = null)
    {
        var result = DoMoveTo(targetShape, options);

        Logger.Debug($"Moved the cursor to the client shape \"{targetShape.Name}\"({targetShape.GetSignature()}).");

        return result;
    }

    public Point MoveTo(Rectangle clientArea, MouseInputOptions? options = null)
    {
        var targetShape = new RectangleShape(clientArea);
        var result = DoMoveTo(targetShape, options);

        Logger.Debug($"Moved the cursor to the client rectangle {{{clientArea.X},{clientArea.Y},{clientArea.Width},{clientArea.Height}}}.");

        return result;
    }

    #endregion


    #region "Mouse down/up"

    public void MouseDown(MouseButtons button)
    {
        //Logger.Debug($"Simulating mouse down: {{{button}}}.");

        DoMouseDown(button);
    }

    public void MouseUp(MouseButtons button)
    {
        //Logger.Debug($"Simulating mouse up: {{{button}}}.");

        DoMouseUp(button);
    }

    #endregion

    #region "Mouse wheel"

    public void WheelForward(int detents, MouseInputOptions? options = null)
    {
        //Logger.Debug($"Simulating mouse wheel scrolling: {detents} detents forward.", options);

        DoVerticalWheel(detents, options);
    }

    public void WheelBackward(int detents, MouseInputOptions? options = null)
    {
        //Logger.Debug($"Simulating mouse wheel scrolling: {detents} detents backward.", options);

        DoVerticalWheel(-detents, options);
    }

    public void WheelLeft(int detents, MouseInputOptions? options = null)
    {
        //Logger.Debug($"Simulating mouse wheel scrolling: {detents} detents left.", options);

        DoHorizontalWheel(-detents, options);
    }

    public void WheelRight(int detents, MouseInputOptions? options = null)
    {
        //Logger.Debug($"Simulating mouse wheel scrolling: {detents} detents right.", options);

        DoHorizontalWheel(detents, options);
    }

    #endregion


    //todo: IsPressed
    public bool IsPressed(MouseButtons button)
    {
        throw new NotImplementedException();
    }

    private Point GetClientCursorPosition()
    {
        var screenPoint = SendInputHelper.Cursor;
        var clientPoint = Locating.PointToClient(screenPoint);
        return clientPoint;
    }

    private void DoDelay(int timeout)
    {
        if (timeout == 0)
        {
            return;
        }

        Thread.Sleep(timeout);
    }

    private Point DoMoveTo(IShape targetShape, MouseInputOptions? options)
    {
        var keepUnmovedInShape = options?.KeepUnmovedInShape ?? DefaultOptions?.KeepUnmovedInShape ?? false;
        var currentPoint = GetClientCursorPosition();
        if (keepUnmovedInShape && targetShape.Contains(currentPoint))
        {
            Logger.Trace($"Skipped moving the cursor because it is currently in the shape.", new { targetShape, options });
            return currentPoint;
        }
        
        var distribution = options?.ShapeDistribution ?? DefaultOptions?.ShapeDistribution ?? default;
        var clientPoint = Distribution.GetPointByShape(targetShape, distribution);
        var screenPoint = Locating.PointToScreen(clientPoint);
        var motion = options?.Motion ?? DefaultOptions?.Motion ?? MouseMoveMotion.Jump;
        DoMoveTo(screenPoint, motion);

        Logger.Trace($"Simulated a mouse move action.", new { targetShape, options, distribution, clientPoint, screenPoint, motion });

        return clientPoint;
    }

    private Point DoMoveTo(Point targetPoint, MouseInputOptions? options)
    {
        var offsetRange = options?.PointOffsetRange ?? DefaultOptions?.PointOffsetRange ?? 0;
        var clientPoint = Distribution.GetPointByOffset(targetPoint, offsetRange);
        var screenPoint = Locating.PointToScreen(clientPoint);
        var motion = options?.Motion ?? DefaultOptions?.Motion ?? MouseMoveMotion.Jump;
        DoMoveTo(screenPoint, motion);

        Logger.Trace($"Simulated a mouse move action.", new { targetPoint, options, offsetRange, clientPoint, screenPoint, motion });

        return clientPoint;
    }

    private void DoMoveTo(Point screenPoint, MouseMoveMotion motion)
    {
        switch (motion)
        {
            case MouseMoveMotion.Jump:
                SendInputHelper.Cursor = screenPoint;
                Logger.Trace($"Moved the cursor to {screenPoint.X}, {screenPoint.Y}.", new { screenPoint, motion });
                break;
            case MouseMoveMotion.Linear:
                var current = SendInputHelper.Cursor;
                var points = CursorHelper.GetLinearPositions(current, screenPoint);
                foreach (var point in points)
                {
                    SendInputHelper.Cursor = point;
                    Logger.Trace($"Moved the cursor to {point.X}, {point.Y}.", new { screenPoint, motion });

                    var interval = 15;
                    DoDelay(interval);
                    Logger.Trace($"Delayed for {interval}ms.");
                }
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private void DoClick(MouseButtons button, MouseInputOptions? options)
    {
        var (min, max) = options?.ClickDuration ?? DefaultOptions?.ClickDuration ?? (0, 0);

        if (min > 0 && max >= min)
        {
            var interval = Distribution.Random.Next(min, max);

            new SendInputHelper().AddMouseDown(button).Execute();
            Logger.Trace($"Simulated a mouse down message.", new { button });

            DoDelay(interval);
            Logger.Trace($"Delayed for {interval}ms.", new { min, max });

            new SendInputHelper().AddMouseUp(button).Execute();
            Logger.Trace($"Simulated a mouse up message.", new { button });
        }
        else
        {
            if (min < 0 || max < 0 || max < min)
            {
                Logger.Warn($"The {nameof(MouseInputOptions.ClickDuration)}({min},{max}) is not set correctly. The value of (0,0) will be used instead.");
            }

            new SendInputHelper()
                .AddMouseDown(button)
                .AddMouseUp(button)
                .Execute();
            Logger.Trace($"Simulated a mouse click message.", new { button, min, max });
        }
    }

    private void DoDoubleClick(MouseButtons button, MouseInputOptions? options)
    {
        var (min1, max1) = options?.ClickDuration ?? DefaultOptions?.ClickDuration ?? (0, 0);
        var (min2, max2) = options?.DoubleClickInterval ?? DefaultOptions?.DoubleClickInterval ?? (0, 0);

        if (min1 > 0 && max1 >= min1 && min2 > 0 && max2 >= min2)
        {
            var interval1 = Distribution.Random.Next(min1, max1);
            var interval2 = Distribution.Random.Next(min2, max2);
            var interval3 = Distribution.Random.Next(min1, max1);

            new SendInputHelper().AddMouseDown(button).Execute();
            Logger.Trace($"Simulated a mouse down message.", new { button });

            DoDelay(interval1);
            Logger.Trace($"Delayed for {interval1}ms.", new { min1, max1 });

            new SendInputHelper().AddMouseUp(button).Execute();
            Logger.Trace($"Simulated a mouse up message.", new { button });

            DoDelay(interval2);
            Logger.Trace($"Delayed for {interval2}ms.", new { min2, max2 });

            new SendInputHelper().AddMouseDown(button).Execute();
            Logger.Trace($"Simulated a mouse down message.", new { button });

            DoDelay(interval3);
            Logger.Trace($"Delayed for {interval3}ms.", new { min1, max1 });

            new SendInputHelper().AddMouseUp(button).Execute();
            Logger.Trace($"Simulated a mouse up message.", new { button });
        }
        else
        {
            if (min1 < 0 || max1 < 0 || max1 < min1 || min2 < 0 || max2 < 0 || max1 < min2)
            {
                Logger.Warn($"The {nameof(MouseInputOptions.ClickDuration)}({min1},{max1}) or the {nameof(MouseInputOptions.DoubleClickInterval)}({min2},{max2}) is not set correctly. The values of (0,0) and (0,0) will be used instead.");
            }

            new SendInputHelper()
                .AddMouseDown(button)
                .AddMouseUp(button)
                .AddMouseDown(button)
                .AddMouseUp(button)
                .Execute();
            Logger.Trace($"Simulated a mouse double-click message.", new { button });
        }
        
    }

    private void DoMouseDown(MouseButtons button)
    {
        new SendInputHelper().AddMouseDown(button).Execute();
        Logger.Trace($"Simulated a mouse down message.", new { button });
    }

    private void DoMouseUp(MouseButtons button)
    {
        new SendInputHelper().AddMouseUp(button).Execute();
        Logger.Trace($"Simulated a mouse up message.", new { button });
    }

    private void DoVerticalWheel(int detents, MouseInputOptions? options = null)
    {
        var (min, max) = options?.VerticalWheelInterval ?? DefaultOptions?.VerticalWheelInterval ?? (0, 0);

        if (min == 0 || max == 0)
        {
            var movement = detents * 120;
            new SendInputHelper().AddMouseWheel(movement).Execute();

            Logger.Trace($"Simulated a mouse wheel message.", new { detents, movement });
        }
        else
        {
            var delta = 120;
            var interval = Distribution.Random.Next(min, max);
            var singleDetent = Math.Sign(detents);
            var absoluteDetent = Math.Abs(detents);
            var movement = singleDetent * delta;
            for (var i = 0; i < absoluteDetent; i++)
            {
                new SendInputHelper().AddMouseWheel(movement).Execute();
                Logger.Trace($"Simulated a mouse wheel message.", new { detents, movement });

                if (i < absoluteDetent - 1)
                {
                    DoDelay(interval);
                    Logger.Trace($"Delayed for {interval}ms.", new { min, max });
                }
            }
        }
    }

    private void DoHorizontalWheel(int detents, MouseInputOptions? options = null)
    {
        var (min, max) = options?.HorizontalWheelInterval ?? DefaultOptions?.HorizontalWheelInterval ?? (0, 0);

        if (min == 0 || max == 0)
        {
            var movement = detents * 120;
            new SendInputHelper().AddMouseHWheel(movement).Execute();
            Logger.Trace($"Simulated a mouse h-wheel message.", new { detents, movement });
        }
        else
        {
            var delta = 120;
            var interval = Distribution.Random.Next(min, max);
            var singleDetent = Math.Sign(detents);
            var absoluteDetent = Math.Abs(detents);
            var movement = singleDetent * delta;
            for (var i = 0; i < absoluteDetent; i++)
            {
                new SendInputHelper().AddMouseHWheel(movement).Execute();
                Logger.Trace($"Simulated a mouse h-wheel message.", new { detents, movement });

                if (i < absoluteDetent - 1)
                {
                    DoDelay(interval);
                    Logger.Trace($"Delayed for {interval}ms.", new { min, max });
                }
            }
        }
    }

}
