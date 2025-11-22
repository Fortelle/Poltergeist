using System.Drawing;

namespace Poltergeist.Operations.Locating;

public class LocatedWindowInfo
{
    public nint Handle => ChildHandle ?? ParentHandle ?? nint.Zero;

    public nint? ParentHandle { get; set; }
    public string? ParentWindowName { get; set; }
    public string? ParentClassName { get; set; }

    public nint? ChildHandle { get; set; }
    public string? ChildWindowName { get; set; }
    public string? ChildClassName { get; set; }

    public Rectangle ClientArea { get; set; }

    public override string ToString()
    {
        if (ParentHandle is null)
        {
            return $"Screen ({ClientArea.Width},{ClientArea.Height})";
        }
        var text = $"Window {ParentHandle:X8} \"{ParentWindowName}\" {ParentClassName}";
        if (ChildHandle is not null)
        {
            text += $" > Window {ChildHandle:X8} \"{ChildWindowName}\" {ChildClassName}";
        }
        return text;
    }
}
