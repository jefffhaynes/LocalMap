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

        public static bool Intersects(this Rect r, Rect rectangle)
        {
            r.Intersect(rectangle);
            return !r.IsEmpty;
        }
    }
}
