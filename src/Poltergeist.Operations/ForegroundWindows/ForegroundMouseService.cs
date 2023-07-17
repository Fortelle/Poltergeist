using System.Drawing;
using Microsoft.Extensions.Options;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Common.Structures.Shapes;
using Poltergeist.Common.Utilities.Maths;
using Poltergeist.Input.Windows;

namespace Poltergeist.Operations.ForegroundWindows;

public class ForegroundMouseService : MacroService
{
    private ForegroundLocatingService Locating { get; }
    private MouseInputOptions DefaultOptions { get; }
    private DistributionService Distribution { get; }

    public ForegroundMouseService(
        MacroProcessor processor,
        ForegroundLocatingService locating,
        IOptions<MouseInputOptions> options,
        DistributionService random
        )
        : base(processor)
    {
        Locating = locating;
        DefaultOptions = options.Value;
        Distribution = random;

        Logger.Debug($"Initialized <{nameof(ForegroundMouseService)}>.", DefaultOptions);
    }


    #region "click"

    public void Click(MouseButtons button, MouseInputOptions? options = null)
    {
        //Logger.Debug($"Simulating mouse click: {{{button}}}.", options);

        DoClick(button, options);
    }

    public void Click()
    {
        Click(MouseButtons.Left);
    }

    public void Click(Rectangle targetRectangle)
    {
        MoveTo(targetRectangle);
        Click();
    }

    public void Click(IShape targetShape)
    {
        MoveTo(targetShape);
        Click();
    }

    public void Click(Point targetPoint)
    {
        MoveTo(targetPoint);
        Click();
    }

    #endregion


    #region "double click"

    public void DoubleClick(MouseButtons button, MouseInputOptions? options = null)
    {
        //Logger.Debug($"Simulating mouse double-click: {{{button}}}.", options);

        DoDoubleClick(button, options);
    }

    public void DoubleClick()
    {
        DoubleClick(MouseButtons.Left);
    }

    #endregion


    //todo: LineTo
    #region "move"

    public void MoveBy(int dx, int dy, MouseInputOptions? options = null)
    {
        //Logger.Debug($"Simulating mouse move: to offset({dx},{dy}).", options);

        var clientPoint = GetClientCursorPosition();
        clientPoint.Offset(dx, dy);
        DoMoveTo(clientPoint, options);
    }

    public void MoveTo(Point clientPoint, MouseInputOptions? options = null)
    {
        //Logger.Debug($"Simulating mouse move: to point({clientPoint.X},{clientPoint.Y}).", options);

        DoMoveTo(clientPoint, options);
    }

    public void MoveTo(int x, int y, MouseInputOptions? options = null)
    {
        MoveTo(new Point(x, y), options);
    }

    public void MoveTo(IShape targetShape, MouseInputOptions? options = null)
    {
        //Logger.Debug($"Simulating mouse move: to shape \"{targetShape.Name}\"({targetShape.GetSignature()}).", options);

        DoMoveTo(targetShape, options);
    }

    public void MoveTo(Rectangle clientArea, MouseInputOptions? options = null)
    {
        //Logger.Debug($"Simulating mouse move: to rectangle({clientArea.X},{clientArea.Y},{clientArea.Width},{clientArea.Height}).", options);

        var targetShape = new RectangleShape(clientArea);
        DoMoveTo(targetShape, options);
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

        DoHorizonWheel(-detents, options);
    }

    public void WheelRight(int detents, MouseInputOptions? options = null)
    {
        //Logger.Debug($"Simulating mouse wheel scrolling: {detents} detents right.", options);

        DoHorizonWheel(detents, options);
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
        var clientPoint = Locating.PointToScreen(screenPoint);
        return clientPoint;
    }

    private static void DoDelay(int timeout)
    {
        if (timeout == 0)
        {
            return;
        }

        Thread.Sleep(timeout);
    }

    private void DoMoveTo(IShape targetShape, MouseInputOptions? options)
    {
        var distribution = options?.ShapeDistribution ?? DefaultOptions?.ShapeDistribution ?? default;
        var clientPoint = Distribution.GetPointByShape(targetShape, distribution);
        var screenPoint = Locating.PointToScreen(clientPoint);
        var motion = options?.Motion ?? DefaultOptions?.Motion ?? MouseMoveMotion.Jump;
        CursorHelper.MoveTo(screenPoint, motion);
        Logger.Debug($"Simulated mouse move: to screen({screenPoint.X},{screenPoint.Y}).", new { targetShape, distribution, clientPoint, screenPoint, motion });
    }

    private void DoMoveTo(Point targetPoint, MouseInputOptions? options)
    {
        var offsetRange = options?.PointOffsetRange ?? DefaultOptions?.PointOffsetRange ?? 0;
        var clientPoint = Distribution.GetPointByOffset(targetPoint, offsetRange);
        var screenPoint = Locating.PointToScreen(clientPoint);
        var motion = options?.Motion ?? DefaultOptions?.Motion ?? MouseMoveMotion.Jump;
        CursorHelper.MoveTo(screenPoint, motion);
        Logger.Debug($"Simulated mouse move: to screen({screenPoint.X},{screenPoint.Y}).", new { targetPoint, offsetRange, clientPoint, screenPoint, motion });
    }

    private void DoClick(MouseButtons button, MouseInputOptions? options)
    {
        var (min, max) = options?.ClickTime ?? DefaultOptions?.ClickTime ?? (0, 0);

        if (min == 0 || max == 0)
        {
            new SendInputHelper()
                .AddMouseDown(button)
                .AddMouseUp(button)
                .Execute();
            Logger.Debug($"Simulated mouse click: {{{button}}}.");
        }
        else
        {
            var interval = Distribution.Random.Next(min, max);
            new SendInputHelper().AddMouseDown(button).Execute();
            DoDelay(interval);
            new SendInputHelper().AddMouseUp(button).Execute();
            Logger.Debug($"Simulated mouse click: {{{button}}}.", new { interval });
        }
    }

    private void DoDoubleClick(MouseButtons button, MouseInputOptions? options)
    {
        var (min1, max1) = options?.ClickTime ?? DefaultOptions?.ClickTime ?? (0, 0);
        var (min2, max2) = options?.DoubleClickTime ?? DefaultOptions?.DoubleClickTime ?? (0, 0);

        if (min1 == 0 || min2 == 0)
        {
            new SendInputHelper()
                .AddMouseDown(button)
                .AddMouseUp(button)
                .AddMouseDown(button)
                .AddMouseUp(button)
                .Execute();
            Logger.Debug($"Simulated mouse double-click: {{{button}}}.");
        }
        else
        {
            var interval1 = Distribution.Random.Next(min1, max1);
            var interval2 = Distribution.Random.Next(min2, max2);
            var interval3 = Distribution.Random.Next(min1, max1);

            new SendInputHelper().AddMouseDown(button).Execute();
            DoDelay(interval1);
            new SendInputHelper().AddMouseUp(button).Execute();
            DoDelay(interval2);
            new SendInputHelper().AddMouseDown(button).Execute();
            DoDelay(interval3);
            new SendInputHelper().AddMouseUp(button).Execute();
            Logger.Debug($"Simulated mouse double-click: {{{button}}}.", new { interval1, interval2, interval3 });
        }
    }

    private void DoMouseDown(MouseButtons button)
    {
        new SendInputHelper().AddMouseDown(button).Execute();
        Logger.Debug($"Simulated mouse down: {{{button}}}.");
    }

    private void DoMouseUp(MouseButtons button)
    {
        new SendInputHelper().AddMouseUp(button).Execute();
        Logger.Debug($"Simulated mouse up: {{{button}}}.");
    }

    private void DoVerticalWheel(int detents, MouseInputOptions? options = null)
    {
        var (min, max) = options?.VerticalWheelInterval ?? DefaultOptions?.VerticalWheelInterval ?? (0, 0);

        if (min == 0 || max == 0)
        {
            var movement = detents * 120;
            new SendInputHelper().AddMouseWheel(movement).Execute();
            Logger.Debug($"Simulated mouse wheel scrolling.", new { detents, movement });
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
                if (i < absoluteDetent - 1)
                {
                    DoDelay(interval);
                }
            }
            Logger.Debug($"Simulated mouse wheel scrolling.", new { detents, delta, interval });
        }
    }

    private void DoHorizonWheel(int detents, MouseInputOptions? options = null)
    {
        var (min, max) = options?.HorizonWheelInterval ?? DefaultOptions?.HorizonWheelInterval ?? (0, 0);

        if (min == 0 || max == 0)
        {
            var movement = detents * 120;
            new SendInputHelper().AddMouseHWheel(movement).Execute();
            Logger.Debug($"Simulated mouse h-wheel scrolling.", new { detents, movement });
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
                if (i < absoluteDetent - 1)
                {
                    DoDelay(interval);
                }
            }
            Logger.Debug($"Simulated mouse h-wheel scrolling.", new { detents, delta, interval });
        }
    }

}
