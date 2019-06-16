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
            var latitudeRadians = DegreeToRadian(position.Latitude);

            return new Tile
            (
                (int) (n * ((position.Longitude + 180) / 360)),
                (int) (n * (1 - Math.Log(Math.Tan(latitudeRadians) + 1 / Math.Cos(latitudeRadians)) / Math.PI) / 2),
                zoomLevel
            );
        }

        public Geoposition GetPosition()
        {
            var n = Math.PI - 2.0 * Math.PI * Y / Math.Pow(2.0, ZoomLevel);
            return new Geoposition((float) (X / Math.Pow(2.0, ZoomLevel) * 360.0 - 180.0),
                (float) (180.0 / Math.PI * Math.Atan(Math.Sinh(n))));
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

        private static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
}