using System.Drawing;

namespace Poltergeist.Operations.Inputing;

public record WCSPoint(Point ToWorkspace, Point ToClient, Point ToScreen)
{
}
