using System.Drawing;
using Poltergeist.Operations.Inputting;

namespace Poltergeist.Android.HybridEmulators;

public interface IHybridInputService
{
    Point Tap(PositionToken position);
    Point LongTap(PositionToken position);
    Point Swipe(PositionToken beginPosition, PositionToken endPosition);
    Point GetTargetPoint(PositionToken position);
}
