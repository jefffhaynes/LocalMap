﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

        public static bool DrawTextOnSegments(this CanvasDrawingSession session, string name, List<Vector2> vectors, Color textColor,
            CanvasTextFormat format, double maxRotation = Math.PI / 2)
        {
            var start = vectors[0];

            bool success = false;
            int charIndex = 0;
            foreach (var point in vectors.Skip(1))
            {
                if (DrawTextOnSegment(session, name, textColor, format, ref charIndex,
                    ref start, point, maxRotation))
                {
                    success = true;
                }
                else
                {
                    success = false;
                    break;
                }

                if (charIndex >= name.Length)
                {
                    break;
                }
            }

            return success;
        }

        private static bool DrawTextOnSegment(CanvasDrawingSession session, string value, Color color, CanvasTextFormat format, ref int characterIndex,
            ref Vector2 start, Vector2 end, double maxRotation)
        {
            float dx = end.X - start.X;
            float dy = end.Y - start.Y;
            float distance = Vector2.Distance(start, end);
            dx /= distance;
            dy /= distance;

            // See how many characters will fit.
            int lastCharacterIndex;
            for (lastCharacterIndex = value.Length; lastCharacterIndex > 0; lastCharacterIndex--)
            {
                string testValue =
                    value.Substring(characterIndex, lastCharacterIndex - characterIndex);

                var textLayout = new CanvasTextLayout(session, testValue, format, 0, 0);

                if (textLayout.DrawBounds.Width < distance)
                {
                    break;
                }
            }

            //int last_ch = characterIndex;
            //while (last_ch < value.Length)
            //{
            //    string test_string =
            //        value.Substring(characterIndex, last_ch - characterIndex + 1);
            //    var textLayout = new CanvasTextLayout(session, test_string, format, 0, 0);

            //    if (textLayout.DrawBounds.Width > dist)
            //    {
            //        // This is one too many characters.
            //        last_ch--;
            //        break;
            //    }
            //    last_ch++;
            //}

            if (lastCharacterIndex < characterIndex)
            {
                // won't fit
                return false;
            }

            if (lastCharacterIndex >= value.Length)
            {
                lastCharacterIndex = value.Length - 1;
            }

            string substring =
                value.Substring(characterIndex, lastCharacterIndex - characterIndex + 1);

            var transform = session.Transform;

            var fitTextLayout = new CanvasTextLayout(session, substring, format, 0, 0);

            float angle = (float)Math.Atan2(dy, dx);

            if (Math.Abs(angle) > maxRotation)
            {
                return false;
            }

            var rotation = Matrix3x2.CreateRotation(angle, start);
            session.Transform = Matrix3x2.Multiply(session.Transform, rotation);

            // Draw the characters that fit.
            session.DrawText(substring, start, color, format);

            // Restore the saved state.
            session.Transform = transform;

            // Update characterIndex and start.
            characterIndex = lastCharacterIndex + 1;

            var width = (float)fitTextLayout.DrawBounds.Width;

            start = new Vector2(start.X + dx * width, start.Y + dy * width);

            return true;
        }

    }
}
