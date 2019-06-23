using System;
using Windows.Foundation;

namespace LocalMap
{
    public class Geobounds
    {
        public Geobounds(Geoposition position, double width, double height)
        {
            Position = position;
            Width = width;
            Height = height;
        }

        public Geobounds(double longitude, double latitude, double width, double height) : this(
            new Geoposition(longitude, latitude), width, height)
        {
        }

        public Geoposition Position { get; }
        
        public double Width { get; }

        public double Height { get; }

        public double Left => Position.Longitude;

        public double Right => Position.Longitude + Width;

        public double Top => Position.Latitude;

        public double Bottom => Position.Latitude - Height;

        public Geoposition TopLeft => new Geoposition(Left, Top);

        public Geoposition TopRight => new Geoposition(Right, Top);

        public Geoposition BottomLeft => new Geoposition(Left, Bottom);

        public Geoposition BottomRight => new Geoposition(Right, Bottom);

        public Geobounds Intersect(Geobounds geobounds)
        {
            var longitude = Math.Clamp(Position.Longitude, geobounds.Left, geobounds.Right);
            var latitude = Math.Clamp(Position.Latitude, geobounds.Bottom, geobounds.Top);
            var right = Math.Clamp(Right, geobounds.Left, geobounds.Right);
            var bottom = Math.Clamp(Bottom, geobounds.Bottom, geobounds.Top);

            return new Geobounds(longitude, latitude, right - longitude, latitude - bottom);
        }

        public override string ToString()
        {
            return $"{Position.Latitude:F}, {Position.Longitude:F}, {Bottom:F}, {Right:F}";
        }
    }
}
