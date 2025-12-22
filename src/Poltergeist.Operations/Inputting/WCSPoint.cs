using System.Drawing;

namespace Poltergeist.Operations.Inputting;

public record WCSPoint(Point ToWorkspace, Point ToClient, Point ToScreen)
{
}
