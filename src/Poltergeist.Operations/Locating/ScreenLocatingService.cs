using System.Drawing;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Utilities.Windows;

namespace Poltergeist.Operations.Locating;

public class ScreenLocatingService(MacroProcessor processor) : LocatingProvider(processor)
{
    public Point ClientLocation { get; protected set; }

    public bool TryLocate(RegionConfig config, LocatedWindowInfo? info = null)
    {
        Logger.Trace("Locating the screen region.", config);

        var result = info is null ? Locate(config, out info) : LocateResult.Succeeded;

        Success = result == LocateResult.Succeeded;
        if (Success)
        {
            ClientSize = info!.ClientArea.Size;
            ClientLocation = info.ClientArea.Location;
            WorkspaceSize = config.WorkspaceSize;
            Processor.SessionStorage.AddOrUpdate(WorkspaceSizeKey, WorkspaceSize ?? ClientSize);

            Logger.Info($"Located the specified screen region: {info}({info.ClientArea.X},{info.ClientArea.Y})({info.ClientArea.Width},{info.ClientArea.Height}).");
        }
        else
        {
            ClientSize = default;
            ClientLocation = default;
            WorkspaceSize = null;
            Processor.SessionStorage.TryRemove(WorkspaceSizeKey);

            Logger.Warn(result switch
            {
                LocateResult.NotFound => $"The specified window was not found.",
                LocateResult.ChildNotFound => $"The specified window [{info}] was found, but the child window with class name \"{config.ChildClassName}\" was not found.",
                LocateResult.Minimized => $"The specified window [{info}] is minimized.",
                LocateResult.SizeNotMatch => $"The size of the located window({info?.ClientArea.Size}) does not match the expected workspace size({config.WorkspaceSize}).",
                _ => $"Failed to locate the specified screen region: {result}.",
            });
        }

        return Success;
    }

    public Point WorkspacePointToScreen(Point pointOnWorkspace)
    {
        var pointOnClient = WorkspacePointToClient(pointOnWorkspace);
        var pointOnScreen = ClientPointToScreen(pointOnClient);
        return pointOnScreen;
    }

    public Point ScreenPointToWorkspace(Point pointOnScreen)
    {
        var pointOnClient = ScreenPointToClient(pointOnScreen);
        var pointOnWorkspace = ClientPointToWorkspace(pointOnClient);
        return pointOnWorkspace;
    }

    public Rectangle WorkspaceRectangleToScreen(Rectangle rectangleOnWorkspace)
    {
        var rectangleOnClient = WorkspaceRectangleToClient(rectangleOnWorkspace);
        var rectangleOnScreen = ClientRectangleToScreen(rectangleOnClient);
        return rectangleOnScreen;
    }

    public Point ClientPointToScreen(Point pointOnClient)
    {
        ThrowIfNotReady();

        var x = ClientLocation.X + pointOnClient.X;
        var y = ClientLocation.Y + pointOnClient.Y;
        var pointOnScreen = new Point(x, y);
        return pointOnScreen;
    }

    public Rectangle ClientRectangleToScreen(Rectangle rectangleOnClient)
    {
        ThrowIfNotReady();

        var rectangOnScreen = new Rectangle()
        {
            X = rectangleOnClient.X + ClientLocation.X,
            Y = rectangleOnClient.Y + ClientLocation.Y,
            Width = rectangleOnClient.Width,
            Height = rectangleOnClient.Height,
        };
        return rectangOnScreen;
    }

    public Point ScreenPointToClient(Point pointOnScreen)
    {
        ThrowIfNotReady();

        var x = pointOnScreen.X - ClientLocation.X;
        var y = pointOnScreen.Y - ClientLocation.Y;
        var pointOnClient = new Point(x, y);
        return pointOnClient;
    }

    public static LocateResult Locate(RegionConfig config, out LocatedWindowInfo? info)
    {
        if (config.ClassName is null && config.ProcessName is null && config.WindowName is null && config.Handle == nint.Zero)
        {
            info = new()
            {
                ClientArea = new(default, WindowsFinder.GetScreenSize())
            };
            return LocateResult.Succeeded;
        }

        return WindowLocatingService.Locate(config, out info);
    }
}
