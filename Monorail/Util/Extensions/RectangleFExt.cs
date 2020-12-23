using OpenTK.Mathematics;
using System.Drawing;

namespace Monorail.Util
{
    public static class RectangleFExt
    {
        public static bool IsInside(this RectangleF rec, in Vector2 point)
        {
            return point.X >= rec.X && point.X <= rec.X + rec.Width && point.Y >= rec.Y && point.Y <= rec.Y + rec.Height;
        }
    }
}
