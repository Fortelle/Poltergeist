using System.Drawing;
using Poltergeist.Automations.Processors;
using Poltergeist.Automations.Services;

namespace Poltergeist.Operations.Locating;

public abstract class LocatingProvider(MacroProcessor processor) : MacroService(processor)
{
    public const string WorkspaceSizeKey = "workspace_size";

    public bool Success { get; protected set; }

    public Size? ClientSize { get; protected set; }

    public Size? WorkspaceSize { get; protected set; }

    public Size GetWorkspaceSize() => WorkspaceSize ?? ClientSize ?? Size.Empty;

    public Point WorkspacePointToClient(Point pointOnWorkspace)
    {
        ThrowIfNotReady();

        if (ClientSize is null || WorkspaceSize is null)
        {
            return pointOnWorkspace;
        }
        else
        {
            var x = pointOnWorkspace.X * ClientSize.Value.Width / WorkspaceSize.Value.Width;
            var y = pointOnWorkspace.Y * ClientSize.Value.Height / WorkspaceSize.Value.Height;
            return new Point(x, y);
        }
    }

    public Size WorkspaceSizeToClient(Size sizeOnWorkspace)
    {
        ThrowIfNotReady();

        if (ClientSize is null || WorkspaceSize is null)
        {
            return sizeOnWorkspace;
        }
        else
        {
            var w = sizeOnWorkspace.Width * ClientSize.Value.Width / WorkspaceSize.Value.Width;
            var h = sizeOnWorkspace.Height * ClientSize.Value.Height / WorkspaceSize.Value.Height;
            return new Size(w, h);
        }
    }

    public Point ClientPointToWorkspace(Point pointOnClient)
    {
        ThrowIfNotReady();

        if (ClientSize is null || WorkspaceSize is null)
        {
            return pointOnClient;
        }
        else
        {
            var x = pointOnClient.X * WorkspaceSize.Value.Width / ClientSize.Value.Width;
            var y = pointOnClient.Y * WorkspaceSize.Value.Height / ClientSize.Value.Height;
            return new Point(x, y);
        }
    }

    public Size ClientSizeToWorkspace(Size sizeOnClient)
    {
        ThrowIfNotReady();

        if (ClientSize is null || WorkspaceSize is null)
        {
            return sizeOnClient;
        }
        else
        {
            var w = sizeOnClient.Width * WorkspaceSize.Value.Width / ClientSize.Value.Width;
            var h = sizeOnClient.Height * WorkspaceSize.Value.Height / ClientSize.Value.Height;
            return new Size(w, h);
        }
    }

    public Rectangle WorkspaceRectangleToClient(Rectangle rectangleOnWorkspace) => new(WorkspacePointToClient(rectangleOnWorkspace.Location), WorkspaceSizeToClient(rectangleOnWorkspace.Size));

    public Rectangle ClientRectangleToWorkspace(Rectangle rectangleOnClient) => new(ClientPointToWorkspace(rectangleOnClient.Location), ClientSizeToWorkspace(rectangleOnClient.Size));

    protected void ThrowIfNotReady()
    {
        if (!Success)
        {
            throw new InvalidOperationException($"The {GetType().Name} is not ready to use.");
        }
    }
}
