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
    }
}
