using System.Drawing;
using Newtonsoft.Json;

namespace Poltergeist.Operations;

[JsonObject]
public struct RegionConfig
{
    public string ProcessName { get; set; }
    public string WindowName { get; set; }
    public string ClassName { get; set; }
    public IntPtr Handle { get; set; }

    public Size OriginSize { get; set; }
    public Rectangle Cropping { get; set; }
    public ResizeRule Resizable { get; set; }

    public string ChildClassName { get; set; }

    public bool BringToFront { get; set; }

    public bool Wildcard { get; set; }
}
