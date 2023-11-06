using System.Drawing;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Input.Windows;

namespace Poltergeist.Operations.Foreground;

public class ForegroundLocatingService : MacroService, ILocatingProvider
{
    public Rectangle ClientRegion { get; private set; }
    public PointF? Scale { get; set; }

    public bool Contains(Point pt) => ClientRegion.Contains(pt);
    public bool Contains(Rectangle rect) => ClientRegion.Contains(rect);
    public bool IntersectsWith(Rectangle rect) => ClientRegion.IntersectsWith(rect);

    public ForegroundLocatingService(MacroProcessor processor) : base(processor)
    {
    }

    public bool Locate(RegionConfig config)
    {
        var result = TryLocate(config, out var client, out var scale);

        if (result == LocateResult.Succeeded)
        {
            ClientRegion = client;
            Scale = scale;
            Logger.Debug($"Found requested region.", new { ClientRegion, Scale });

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
        if(Scale is null)
        {
            return new Point(ClientRegion.X + p.X, ClientRegion.Y + p.Y);
        }
        else
        {
            return new Point((int)(ClientRegion.X * Scale.Value.X) + p.X, (int)(ClientRegion.Y * Scale.Value.Y) + p.Y);
        }
    }

    public Point PointToClient(Point p)
    {
        if (Scale is null)
        {
            return new Point(p.X - ClientRegion.X, p.Y - ClientRegion.Y);
        }
        else
        {
            return new Point(p.X - (int)(ClientRegion.X / Scale.Value.X), p.Y - (int)(ClientRegion.Y / Scale.Value.Y));
        }
    }

    public Rectangle RectangleToScreen(Rectangle rect)
    {
        if (Scale is null)
        {
            return new Rectangle(PointToScreen(rect.Location), rect.Size);
        }
        else
        {
            var size = new Size((int)(rect.Size.Width * Scale.Value.X), (int)(rect.Size.Height * Scale.Value.Y));
            return new Rectangle(PointToClient(rect.Location), size);
        }
    }

    public Rectangle RectangleToClient(Rectangle rect)
    {
        if (Scale is null)
        {
            return new Rectangle(PointToClient(rect.Location), rect.Size);
        }
        else
        {
            var size = new Size((int)(rect.Size.Width / Scale.Value.X), (int)(rect.Size.Height / Scale.Value.Y));
            return new Rectangle(PointToClient(rect.Location), size);
        }
    }

    public static LocateResult TryLocate(RegionConfig config, out Rectangle client, out PointF? scale)
    {
        client = default;
        scale = null;

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
                Thread.Sleep(500);
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
            switch (config.Resizable)
            {
                case ResizeRule.Disallow:
                    return LocateResult.SizeNotMatch;
                case ResizeRule.ConstrainProportion:
                    if ((double)client.Width / client.Height == (double)config.OriginSize.Width / config.OriginSize.Height)
                    {
                        scale = new PointF((float)config.OriginSize.Width / client.Width, (float)config.OriginSize.Height / client.Height);
                    }
                    else
                    {
                        return LocateResult.SizeNotMatch;
                    }
                    break;
                case ResizeRule.AnySize:
                    scale = new PointF((float)config.OriginSize.Width / client.Width, (float)config.OriginSize.Height / client.Height);
                    break;
            }
        }

        else if (config.Cropping != default)
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
