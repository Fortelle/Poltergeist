using System.Drawing;
using Microsoft.Extensions.Options;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Structures.Shapes;
using Poltergeist.Automations.Utilities.Maths;
using Poltergeist.Automations.Utilities.Windows;
using Poltergeist.Operations.Foreground;

namespace Poltergeist.Operations.Background;

public class BackgroundMouseService : MacroService
{
    private readonly BackgroundLocatingService Locating;
    private readonly DistributionService Distribution;
    private readonly MouseInputOptions DefaultOptions;

    public BackgroundMouseService(
        MacroProcessor processor,
        BackgroundLocatingService locating,
        IOptions<MouseInputOptions> options,
        DistributionService distribution
        ) : base(processor)
    {
        Locating = locating;
        Distribution = distribution;
        DefaultOptions = options.Value;
    }

    #region "click"

    public void Click(uint x, uint y, MouseButtons button = MouseButtons.Left, MouseInputOptions? options = null)
    {
        //Logger.Debug($"Simulating mouse click: {{{button}}}.", options);

        DoClick(x, y, button, options);
    }

    #endregion


    #region "double click"

    public void DoubleClick(uint x, uint y, MouseButtons button = MouseButtons.Left, MouseInputOptions? options = null)
    {
        //Logger.Debug($"Simulating mouse double-click: {{{button}}}.", options);

        DoDoubleClick(x, y, button, options);
    }

    #endregion


    #region "Mouse down/up"

    public void MouseDown(uint x, uint y, MouseButtons button)
    {
        //Logger.Debug($"Simulating mouse down: {{{button}}}.");

        DoMouseDown(x, y, button);
    }

    public void MouseUp(uint x, uint y, MouseButtons button)
    {
        //Logger.Debug($"Simulating mouse up: {{{button}}}.");

        DoMouseUp(x, y, button);
    }

    #endregion

    public Point GetPoint(Point targetPoint)
    {
        var offsetRange = DefaultOptions?.PointOffsetRange ?? 0;
        var point = Distribution.GetPointByOffset(targetPoint, offsetRange);
        return point;
    }

    public Point GetPoint(IShape targetShape)
    {
        var distribution = DefaultOptions?.ShapeDistribution ?? default;
        var point = Distribution.GetPointByShape(targetShape, distribution);
        return point;
    }

    public Point GetPoint(Rectangle targetRectangle) => GetPoint(new RectangleShape(targetRectangle));

    private static void DoDelay(int timeout)
    {
        if (timeout == 0)
        {
            return;
        }

        Thread.Sleep(timeout);
    }

    private void DoClick(uint x, uint y, MouseButtons button, MouseInputOptions? options)
    {
        var (min, max) = options?.ClickDuration ?? DefaultOptions?.ClickDuration ?? (0, 0);

        var interval = Distribution.Random.Next(min, max);
        Locating.SendMessage.MouseButtonDown(x, y, button);
        DoDelay(interval);
        Locating.SendMessage.MouseButtonUp(x, y, button);
        Logger.Debug($"Simulated mouse click: {{{button}}}.", new { interval });
    }

    private void DoDoubleClick(uint x, uint y, MouseButtons button, MouseInputOptions? options)
    {
        var (min1, max1) = options?.ClickDuration ?? DefaultOptions?.ClickDuration ?? (0, 0);
        var (min2, max2) = options?.DoubleClickInterval ?? DefaultOptions?.DoubleClickInterval ?? (0, 0);

        var interval1 = Distribution.Random.Next(min1, max1);
        var interval2 = Distribution.Random.Next(min2, max2);
        var interval3 = Distribution.Random.Next(min1, max1);

        Locating.SendMessage.MouseButtonDown(x, y, button);
        DoDelay(interval1);
        Locating.SendMessage.MouseButtonUp(x, y, button);
        DoDelay(interval2);
        Locating.SendMessage.MouseButtonDown(x, y, button);
        DoDelay(interval3);
        Locating.SendMessage.MouseButtonUp(x, y, button);
        Logger.Debug($"Simulated mouse double-click: {{{button}}}.", new { interval1, interval2, interval3 });
    }

    private void DoMouseDown(uint x, uint y, MouseButtons button)
    {
        Locating.SendMessage.MouseButtonDown(x, y, button);
        Logger.Debug($"Simulated mouse down: {{{button}}}.");
    }

    private void DoMouseUp(uint x, uint y, MouseButtons button)
    {
        Locating.SendMessage.MouseButtonUp(x, y, button);
        Logger.Debug($"Simulated mouse up: {{{button}}}.");
    }

}
