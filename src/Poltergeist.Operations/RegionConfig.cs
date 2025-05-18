using System.Drawing;

namespace Poltergeist.Operations;

public class RegionConfig
{
    public string? ProcessName { get; set; }
    public string? WindowName { get; set; }
    public string? ClassName { get; set; }
    public string? ChildClassName { get; set; }

    public int Delay { get; set; }
    public Size? OriginalSize { get; set; }
    public Rectangle? Cropping { get; set; }
    public ResizeRule Resizable { get; set; }
    public bool BringToFront { get; set; }
    public bool Wildcard { get; set; }

    public IntPtr Handle { get; set; }
}
