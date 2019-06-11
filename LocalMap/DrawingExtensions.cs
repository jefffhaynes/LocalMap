using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Mapbox.Vector.Tile;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace LocalMap
{
    public static class DrawingExtensions
    {
        public static Vector2 ToVector2(this Coordinate coordinate, float scale = 1f)
        {
            return new Vector2(coordinate.X * scale, coordinate.Y * scale);
        }

        public static float Measure(this string value, CanvasDrawingSession session, CanvasTextFormat format)
        {
            using (var layout = new CanvasTextLayout(session, value, format, 0, 0))
            {
                return (float) layout.LayoutBoundsIncludingTrailingWhitespace.Width;
            }
        }

        public static void DrawTextOnSegments(this CanvasDrawingSession session, string name, List<Vector2> vectors,
            Color textColor, CanvasTextFormat format, int tileSize, List<Polygon> collisionBoxes, 
            double maxRotation = Math.PI / 2)
        {
            var start = vectors[0];

            int charIndex = 0;
            var textCollisionBoxes = new List<Polygon>();

            format.VerticalAlignment = CanvasVerticalAlignment.Center;

            using (session.CreateLayer(1))
            {
                foreach (var point in vectors.Skip(1))
                {
                    if (DrawTextOnSegment(session, name, textColor, format, ref charIndex,
                        ref start, point, tileSize, collisionBoxes, out var collisionBox, maxRotation))
                    {
                        textCollisionBoxes.Add(collisionBox);
                    }
                    else
                    {
                        break;
                    }

                    if (charIndex >= name.Length)
                    {
                        collisionBoxes.AddRange(textCollisionBoxes);
                        return;
                    }
                }

                // couldn't draw it
                session.Clear(Colors.Transparent);
            }
        }

        private static bool DrawTextOnSegment(CanvasDrawingSession session, string value, Color color,
            CanvasTextFormat format, ref int characterIndex,
            ref Vector2 start, Vector2 end, int tileSize, List<Polygon> collisionBoxes, out Polygon collisionBox, double maxRotation)
        {
            float dx = end.X - start.X;
            float dy = end.Y - start.Y;
            float distance = Vector2.Distance(start, end);
            dx /= distance;
            dy /= distance;

            int lastCharacterIndex = characterIndex;
            while (lastCharacterIndex < value.Length)
            {
                string substring =
                    value.Substring(characterIndex, lastCharacterIndex - characterIndex + 1);

                var length = substring.Measure(session, format);

                if (length > distance)
                {
                    // This is one too many characters.
                    lastCharacterIndex--;
                    break;
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

            string finalSubstring =
                value.Substring(characterIndex, lastCharacterIndex - characterIndex + 1);

            var transform = session.Transform;

            float width;
            using (var layout = new CanvasTextLayout(session, finalSubstring, format, 0, 0))
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

                width = (float)layout.LayoutBoundsIncludingTrailingWhitespace.Width;

                var rect = new Rect(start.X, start.Y, width,
                    layout.LayoutBoundsIncludingTrailingWhitespace.Height);
                var collision = new Polygon(rect).Transform(rotationTransform);
                collisionBox = collision;

                //session.DrawRectangle(rect, Colors.Purple);

                // reset transform
                session.Transform = transform;

                var tileBox = new Rect(0, 0, tileSize, tileSize);
                if (!tileBox.Contains(collision) || collisionBoxes.Any(b => b.Intersects(collision)))
                {
                    // TODO try to move down segments
                    return false;
                }

                // Update characterIndex and start.
                characterIndex = lastCharacterIndex + 1;
            }

            start = new Vector2(start.X + dx * width, start.Y + dy * width);

            return true;
        }
    }
}
