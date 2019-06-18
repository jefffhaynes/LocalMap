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
            var longitude = Math.Max(Position.Longitude, geobounds.Position.Longitude);
            var latitude = Math.Min(Position.Latitude, geobounds.Position.Latitude);
            var right = Math.Min(Right, geobounds.Right);
            var bottom = Math.Max(Bottom, geobounds.Bottom);

            return new Geobounds(longitude, latitude, right - longitude, latitude - bottom);
        }
    }
}
