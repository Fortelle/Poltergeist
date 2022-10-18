using Poltergeist.Automations.Logging;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Input.Windows;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Poltergeist.Operations.ForegroundWindows;

public class ForegroundLocatingService : MacroService
{
    public Rectangle ClientRegion { get; private set; }

    public bool Contains(Point pt) => ClientRegion.Contains(pt);
    public bool Contains(Rectangle rect) => ClientRegion.Contains(rect);
    public bool IntersectsWith(Rectangle rect) => ClientRegion.IntersectsWith(rect);

    public ForegroundLocatingService(MacroProcessor processor) : base(processor)
    {
    }

    public bool Locate(RegionConfig config)
    {
        var result = TryLocate(config, out var client);

        if (result == LocateResult.Succeeded)
        {
            ClientRegion = client;
            Logger.Debug($"Found requested region.", new { ClientRegion });

            return true;
        }
        else
        {
            Logger.Warn($"Failed to find requested region: {result}.");

            return false;
        }
    }

    public Rectangle Intersect(Rectangle rect)
    {
        var bound = ClientRegion;
        bound.Intersect(rect);
        return bound;
    }

    public Point PointToScreen(Point p)
    {
        return new Point(ClientRegion.X + p.X, ClientRegion.Y + p.Y);
    }

    public Point PointToClient(Point p)
    {
        return new Point(p.X - ClientRegion.X, p.Y - ClientRegion.Y);
    }

    public Rectangle RectangleToScreen(Rectangle rect)
    {
        return new Rectangle(PointToScreen(rect.Location), rect.Size);
    }

    public Rectangle RectangleToClient(Rectangle rect)
    {
        return new Rectangle(PointToClient(rect.Location), rect.Size);
    }

    public static LocateResult TryLocate(RegionConfig config, out Rectangle client)
    {
        client = default;

        if (config.ClassName == null && config.ProcessName == null && config.WindowName == null && config.Handle == IntPtr.Zero)
        {
            client.Size = WindowsFinder.GetScreenSize();
        }
        else
        {
            var hwnd = config.Handle != IntPtr.Zero
                ? config.Handle
                : WindowsFinder.FindWindow(config.WindowName, config.ClassName, config.ProcessName);

            if (hwnd == IntPtr.Zero)
            {
                return LocateResult.NotFound;
            }

            var helper = new WindowHelper(hwnd);

            if (helper.IsMinimized)
            {
                if (config.BringToFront)
                {
                    helper.Unminimize();
                }
                else
                {
                    return LocateResult.Minimized;
                }
            }
            if (config.BringToFront)
            {
                helper.BringToFront();
            }

            // ugly
            if (!string.IsNullOrEmpty(config.ChildClassName))
            {
                var path = config.ChildClassName.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                if (!path.All(className =>
                {
                    var children = WindowsFinder.FindChildWindows(hwnd);
                    return children.Any(childHwnd =>
                    {
                        helper = new WindowHelper(childHwnd);
                        return helper.GetClassName() == className;
                    });
                }))
                {
                    return LocateResult.NotFound;
                }
            }

            var rect = helper.GetBounds();
            if (!rect.HasValue)
            {
                return LocateResult.NotFound;
            }
            client = rect.Value;
        }

        if (config.OriginSize != default && client.Size != config.OriginSize)
        {
            return LocateResult.SizeNotMatch;
        }

        if (config.Cropping != default)
        {
            client = new Rectangle(
                client.X + config.Cropping.X,
                client.Y + config.Cropping.Y,
                config.Cropping.Width,
                config.Cropping.Height
            );
        }

        return LocateResult.Succeeded;
    }
}
