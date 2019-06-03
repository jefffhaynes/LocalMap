using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Xaml.Documents;

namespace LocalMap
{
    public static class RectExtensions
    {
        public static Rect Expand(this Rect rect, double value)
        {
            return new Rect(rect.X - value, rect.Y - value, rect.Width + value, rect.Height + value);
        }

        public static bool Contains(this Rect r, Rect rectangle)
        {
            return r.Left < rectangle.Left &&
                   r.Right > rectangle.Right &&
                   r.Top < rectangle.Top &&
                   r.Bottom > rectangle.Bottom;
        }

        public static bool Contains(this Rect rect, Vector2 point)
        {
            return rect.Left < point.X &&
                   rect.Right > point.X &&
                   rect.Top < point.Y &&
                   rect.Bottom > point.Y;
        }

        public static bool Contains(this Rect rect, Polygon polygon)
        {
            return polygon.Points.All(p => rect.Contains(p));
        }
    }
}
