using System.Drawing;
using Poltergeist.Common.Structures.Shapes;

namespace Poltergeist.Operations.AndroidEmulators;

public interface IEmulatorInputSource
{
    public IEmulatorInputSource MoveTo(Rectangle targetRectangle);
    public IEmulatorInputSource MoveTo(IShape targetShape);
    public IEmulatorInputSource MoveTo(Point targetPoint);
    public IEmulatorInputSource MoveTo(int x, int y);

    public IEmulatorInputSource Tap();
    public IEmulatorInputSource LongTap();
    public IEmulatorInputSource Drag();
    public IEmulatorInputSource Drop();
}
