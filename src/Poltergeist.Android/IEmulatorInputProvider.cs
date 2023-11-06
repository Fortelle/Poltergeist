using System.Drawing;
using Poltergeist.Common.Structures.Shapes;

namespace Poltergeist.Android;

public interface IEmulatorInputProvider
{
    public void MoveTo(Rectangle targetRectangle);
    public void MoveTo(IShape targetShape);
    public void MoveTo(Point targetPoint);
    public void MoveTo(int x, int y);

    public void Tap();
    public void LongTap();
    public void Drag();
    public void Drop();
}
