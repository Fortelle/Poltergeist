using System.Drawing;

namespace Poltergeist.Operations.Locating;

public class RegionConfig
{
    public string? ProcessName { get; set; }
    public string? WindowName { get; set; }
    public string? ClassName { get; set; }
    public string? ChildClassName { get; set; }

    public Size? WorkspaceSize { get; set; }
    public Rectangle? Cropping { get; set; }
    public ResizeRule Resizable { get; set; }

    public nint Handle { get; set; }
}
