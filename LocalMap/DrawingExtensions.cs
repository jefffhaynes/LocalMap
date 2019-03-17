using System.Numerics;
using Mapbox.Vector.Tile;

namespace MapTest
{
    public static class DrawingExtensions
    {
        public static Vector2 ToVector2(this Coordinate coordinate, float scale = 1f)
        {
            return new Vector2(coordinate.X * scale, coordinate.Y * scale);
        }
    }
}
