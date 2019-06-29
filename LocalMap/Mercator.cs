using System;
using System.Numerics;
using Windows.Foundation;

namespace LocalMap
{
    public static class Mercator
    {
        public static Geoposition Project(Vector2 position, double mapSize)
        {
            var n = Math.PI - 2.0 * Math.PI * position.Y / mapSize;
            return new Geoposition(180.0 / Math.PI * Math.Atan(Math.Sinh(n)),
                position.X / mapSize * 360.0 - 180.0);
        }

        public static Vector2 ProjectBack(Geoposition position, double mapSize)
        {
            var x = (position.Longitude + 180.0) / 360.0 * mapSize;
            var y = (1.0 - Math.Log(Math.Tan(position.Latitude * Math.PI / 180.0) +
                                    1.0 / Math.Cos(position.Latitude * Math.PI / 180.0)) / Math.PI) / 2.0 * mapSize;

            return new Vector2((float) x, (float) y);
        }

        public static Geobounds Project(Rect bounds, double mapSize)
        {
            var topLeft = Project(new Vector2((float) bounds.X, (float) bounds.Y), mapSize);
            var bottomRight = Project(new Vector2((float) bounds.Right, (float) bounds.Bottom), mapSize);
            return new Geobounds(topLeft, bottomRight);
        }
    }
}
