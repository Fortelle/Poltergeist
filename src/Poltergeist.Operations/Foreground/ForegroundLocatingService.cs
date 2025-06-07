using System.Drawing;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;
using Poltergeist.Automations.Utilities.Windows;

namespace Poltergeist.Operations.Foreground;

public class ForegroundLocatingService : MacroService, ILocatingProvider
{
    private Rectangle? clientRegion;
    public Rectangle ClientRegion
    {
        get
        {
            if (clientRegion is null)
            {
                throw new Exception("The foreground region is not located successfully.");
            }

            return clientRegion.Value;
        }
    }

    public PointF? Scale { get; private set; }

    public ForegroundLocatingService(MacroProcessor processor) : base(processor)
    {
    }

    public bool Locate(RegionConfig config)
    {
        //Logger.Trace($"Trying to find the foreground region.", config);

        Logger.Trace($"Locating region.", config);

        var result = TryLocate(config, out var client, out var scale);

        if (result == LocateResult.Succeeded)
        {
            clientRegion = client;
            Scale = scale;
            Processor.SessionStorage.AddOrUpdate("client_size", client.Size);
            //Logger.Trace($"Found the requested region.", new { ClientRegion, Scale });
            Logger.Trace($"Successfully located region", new { ClientRegion, Scale });

            return true;
        }
        else
        {
            clientRegion = null;
            Scale = null;
            Processor.SessionStorage.TryRemove("client_size");
            //Logger.Trace($"Failed to find the requested region.", new { result });
            Logger.Trace($"Failed to located region.", new { result });

            return false;
        }
    }

    public Point PointToScreen(Point p)
    {
        if (Scale is null)
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

        if (config.Delay > 0)
        {
            Thread.Sleep(config.Delay);
        }

        if (config.ClassName is null && config.ProcessName is null && config.WindowName is null && config.Handle == IntPtr.Zero)
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
                var path = config.ChildClassName.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
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

        if (config.OriginalSize.HasValue && client.Size != config.OriginalSize)
        {
            switch (config.Resizable)
            {
                case ResizeRule.Disallow:
                    return LocateResult.SizeNotMatch;
                case ResizeRule.ConstrainProportion:
                    if ((double)client.Width / client.Height == (double)config.OriginalSize.Value.Width / config.OriginalSize.Value.Height)
                    {
                        scale = new PointF((float)config.OriginalSize.Value.Width / client.Width, (float)config.OriginalSize.Value.Height / client.Height);
                    }
                    else
                    {
                        return LocateResult.SizeNotMatch;
                    }
                    break;
                case ResizeRule.AnySize:
                    scale = new PointF((float)config.OriginalSize.Value.Width / client.Width, (float)config.OriginalSize.Value.Height / client.Height);
                    break;
            }
        }
        else if (config.Cropping is not null)
        {
            client = new Rectangle(
                client.X + config.Cropping.Value.X,
                client.Y + config.Cropping.Value.Y,
                config.Cropping.Value.Width,
                config.Cropping.Value.Height
            );
        }

        return LocateResult.Succeeded;
    }
}
