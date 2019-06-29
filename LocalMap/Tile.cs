using System;
using System.Numerics;

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
            var tilePosition = Mercator.ProjectBack(position, Math.Pow(2, zoomLevel));
            return new Tile((int) tilePosition.X, (int) tilePosition.Y, zoomLevel);
        }

        public Geoposition ToPosition()
        {
            return Mercator.Project(new Vector2(X, Y), Math.Pow(2, ZoomLevel));
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