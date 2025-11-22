using System.Drawing;

namespace Poltergeist.Operations.Capturing;

public class CapturingOptions
{
    public Bitmap? WorkspaceSnapshot { get; set; }
    public bool? RequiresFullSnapshot { get; set; }
}
