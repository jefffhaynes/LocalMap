using Windows.Foundation;

namespace LocalMap
{
    public static class RectExtensions
    {
        public static Rect Expand(this Rect rect, double value)
        {
            return new Rect(rect.X - value, rect.Y - value, rect.Width + value, rect.Height + value);
        }
    }
}
