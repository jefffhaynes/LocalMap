using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Mapbox.Vector.Tile;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;

namespace LocalMap
{
    public static class DrawingExtensions
    {
        public static Vector2 ToVector2(this Coordinate coordinate, float scale = 1f)
        {
            return new Vector2(coordinate.X * scale, coordinate.Y * scale);
        }

        public static bool DrawTextOnSegments(this CanvasDrawingSession session, string name, List<Vector2> vectors,
            Color textColor,
            CanvasTextFormat format, List<Polygon> collisionBoxes, double maxRotation = Math.PI / 2)
        {
            var start = vectors[0];

            bool success = false;
            int charIndex = 0;
            var textCollisionBoxes = new List<Polygon>();

            using (session.CreateLayer(1))
            {
                foreach (var point in vectors.Skip(1))
                {
                    if (DrawTextOnSegment(session, name, textColor, format, ref charIndex,
                        ref start, point, collisionBoxes, out var collisionBox, maxRotation))
                    {
                        textCollisionBoxes.Add(collisionBox);
                        success = true;
                    }
                    else
                    {
                        success = false;
                        session.Clear(Colors.Transparent);
                        break;
                    }

                    if (charIndex >= name.Length)
                    {
                        break;
                    }
                }
            }

            if (success)
            {
                collisionBoxes.AddRange(textCollisionBoxes);
            }

            return success;
        }

        private static bool DrawTextOnSegment(CanvasDrawingSession session, string value, Color color,
            CanvasTextFormat format, ref int characterIndex,
            ref Vector2 start, Vector2 end, List<Polygon> collisionBoxes, out Polygon collisionBox, double maxRotation)
        {
            float dx = end.X - start.X;
            float dy = end.Y - start.Y;
            float distance = Vector2.Distance(start, end);
            dx /= distance;
            dy /= distance;

            int lastCharacterIndex = characterIndex;
            while (lastCharacterIndex < value.Length)
            {
                string testValue =
                    value.Substring(characterIndex, lastCharacterIndex - characterIndex + 1);
                using (var textLayout = new CanvasTextLayout(session, testValue, format, 0, 0))
                {
                    if (textLayout.DrawBounds.Width > distance)
                    {
                        // This is one too many characters.
                        lastCharacterIndex--;
                        break;
                    }
                }

                lastCharacterIndex++;
            }

            if (lastCharacterIndex < characterIndex)
            {
                // won't fit
                collisionBox = Polygon.Empty;
                return false;
            }

            if (lastCharacterIndex >= value.Length)
            {
                lastCharacterIndex = value.Length - 1;
            }

            string substring =
                value.Substring(characterIndex, lastCharacterIndex - characterIndex + 1);

            var transform = session.Transform;

            float width;
            using (var layout = new CanvasTextLayout(session, substring, format, 0, 0))
            {
                float angle = (float)Math.Atan2(dy, dx);

                if (Math.Abs(angle) > maxRotation)
                {
                    collisionBox = Polygon.Empty;
                    return false;
                }

                var rotationTransform = Matrix3x2.CreateRotation(angle, start);
                session.Transform = rotationTransform;

                // Draw the characters that fit.
                session.DrawTextLayout(layout, start, color);

                var rect = new Rect(start.X, start.Y, layout.DrawBounds.Width,
                    layout.DrawBounds.Height);
                var collision = new Polygon(rect).Transform(rotationTransform);
                collisionBox = collision;

                session.Transform = transform;

                if (collisionBoxes.Any(b => b.Intersects(collision)))
                {
                    return false;
                }

                //using (var pathBuilder = new CanvasPathBuilder(session))
                //{
                //    pathBuilder.BeginFigure(collisionBox.Points[0]);
                //    foreach (var boxPoint in collisionBox.Points.Skip(1))
                //    {
                //        pathBuilder.AddLine(boxPoint);
                //    }
                //    pathBuilder.EndFigure(CanvasFigureLoop.Closed);

                //    using (var geo = CanvasGeometry.CreatePath(pathBuilder))
                //    {
                //        session.DrawGeometry(geo,
                //            collisionBoxes.Any(b => b.Intersects(collision)) ? Colors.Red : Colors.Purple);
                //    }
                //}

                // Update characterIndex and start.
                characterIndex = lastCharacterIndex + 1;

                width = (float) layout.DrawBounds.Width;
            }

            start = new Vector2(start.X + dx * width, start.Y + dy * width);

            return true;
        }

    }
}
