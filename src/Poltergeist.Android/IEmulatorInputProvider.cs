using System.Drawing;
using Poltergeist.Automations.Structures.Shapes;

namespace Poltergeist.Android;

public interface IEmulatorInputProvider
{
    void MoveTo(Rectangle targetRectangle);
    void MoveTo(IShape targetShape);
    void MoveTo(Point targetPoint);
    void MoveTo(int x, int y);

    void Tap();
    void LongTap();
    void Drag();
    void Drop();
}
