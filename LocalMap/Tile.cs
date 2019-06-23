using System;

namespace LocalMap
{
    public class Tile : IEquatable<Tile>
    {
        public Tile(int x, int y, int zoomLevel)
        {
            X = x;
            Y = y;
            ZoomLevel = zoomLevel;
        }

        public int X { get; }

        public int Y { get; }

        public int ZoomLevel { get; }

        public bool Equals(Tile other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return X == other.X && Y == other.Y && ZoomLevel == other.ZoomLevel;
        }

        public static Tile FromPosition(Geoposition position, int zoomLevel)
        {
            var n = Math.Pow(2, zoomLevel);
            var x = Math.Floor((position.Longitude + 180.0) / 360.0 * n);
            var y = Math.Floor(
                (1.0 - Math.Log(Math.Tan(position.Latitude * Math.PI / 180.0) +
                                1.0 / Math.Cos(position.Latitude * Math.PI / 180.0)) / Math.PI) / 2.0 * n);

            return new Tile((int) x, (int) y, zoomLevel);
        }

        public Geoposition ToPosition()
        {
            var n = Math.PI - 2.0 * Math.PI * Y / Math.Pow(2.0, ZoomLevel);
            return new Geoposition(180.0 / Math.PI * Math.Atan(Math.Sinh(n)),
                X / Math.Pow(2.0, ZoomLevel) * 360.0 - 180.0);
        }

        public Tile Bounded()
        {
            var x = Math.Max(0, X);
            var y = Math.Max(0, Y);

            var max = (int) Math.Pow(2, ZoomLevel) - 1;
            x = Math.Min(max, x);
            y = Math.Min(max, y);

            return new Tile(x, y, ZoomLevel);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((Tile) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X;
                hashCode = (hashCode * 397) ^ Y;
                hashCode = (hashCode * 397) ^ ZoomLevel;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{X}, {Y}";
        }
    }
}