using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.Foundation;

namespace LocalMap
{
    public class Polygon
    {
        public Polygon(IEnumerable<Vector2> points)
        {
            Points = points.ToList();
        }

        public static Polygon Empty = new Polygon(Enumerable.Empty<Vector2>());

        public Polygon(Rect rectangle)
        {
            var x = (float) rectangle.X;
            var y = (float) rectangle.Y;
            var width = (float) rectangle.Width;
            var height = (float) rectangle.Height;

            Points = new List<Vector2>
            {
                new Vector2(x, y),
                new Vector2(x + width, y),
                new Vector2(x + width, y + height),
                new Vector2(x, y + height)
            };
        }

        public List<Vector2> Points { get; set; }

        public Polygon Transform(Matrix3x2 transform)
        {
            var points = Points.Select(p => Vector2.Transform(p, transform));
            return new Polygon(points);
        }

        public bool Intersects(Polygon polygon)
        {
            foreach (var p in new[] {this, polygon})
            {
                for (var i1 = 0; i1 < p.Points.Count; i1++)
                {
                    var i2 = (i1 + 1) % p.Points.Count;
                    var p1 = p.Points[i1];
                    var p2 = p.Points[i2];

                    var normal = new Point(p2.Y - p1.Y, p1.X - p2.X);

                    double? minA = null, maxA = null;
                    foreach (var point in Points)
                    {
                        var projected = normal.X * point.X + normal.Y * point.Y;
                        if (minA == null || projected < minA)
                        {
                            minA = projected;
                        }

                        if (maxA == null || projected > maxA)
                        {
                            maxA = projected;
                        }
                    }

                    double? minB = null, maxB = null;
                    foreach (var point in polygon.Points)
                    {
                        var projected = normal.X * point.X + normal.Y * point.Y;
                        if (minB == null || projected < minB)
                        {
                            minB = projected;
                        }

                        if (maxB == null || projected > maxB)
                        {
                            maxB = projected;
                        }
                    }

                    if (maxA < minB || maxB < minA)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}