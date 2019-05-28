using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI;
using Mapbox.Vector.Tile;
using MapboxStyle;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;

namespace MapTest
{
    public static class TileRasterer
    {
        const float TileSize = 512;

        private static readonly MemoryCache Cache = new MemoryCache(new MemoryCacheOptions());

        public static async Task<IRandomAccessStream> RasterAsync(Tile tile, List<VectorTileLayer> layers, Style style)
        {
            var imageKey = tile;

            if (!Cache.TryGetValue(imageKey, out InMemoryRandomAccessStream stream))
            {
                using (var canvasDevice = new CanvasDevice())
                {
                    using (var canvasRenderTarget = new CanvasRenderTarget(canvasDevice, TileSize, TileSize, 96))
                    {
                        using (var session = canvasRenderTarget.CreateDrawingSession())
                        {
                            session.Antialiasing = CanvasAntialiasing.Antialiased;
                            session.TextAntialiasing = CanvasTextAntialiasing.ClearType;

                            var background = Convert(style.Background?.Paint?.BackgroundColor?.GetValue(0));

                            if (background != null)
                            {
                                session.FillRectangle(new Rect(0, 0, TileSize, TileSize), background.Value);
                            }

                            List<Rect> boxes = new List<Rect>();
                            foreach (var layer in layers)
                            {
                                RasterLayer(session, canvasRenderTarget, tile, layer, style, TileSize, boxes);
                            }
                        }

                        stream = new InMemoryRandomAccessStream();

                        await canvasRenderTarget.SaveAsync(stream, CanvasBitmapFileFormat.Png);

                        Cache.Set(imageKey, stream);
                    }
                }
            }

            return stream;
        }

        private static void RasterLayer(CanvasDrawingSession session, CanvasRenderTarget canvasRenderTarget, Tile tile,
            VectorTileLayer layer, Style style, float size, List<Rect> boxes)
        {
            var styleLayers = style.GetLayers(layer.Name, tile.ZoomLevel).ToList();

            var scale = size / layer.Extent;

            List<string> names = new List<string>();
            foreach (var feature in layer.VectorTileFeatures)
            {
                RasterFeature(session, canvasRenderTarget, tile, feature, styleLayers, scale, boxes,names);
            }
        }

        private static void RasterFeature(CanvasDrawingSession session, CanvasRenderTarget canvasRenderTarget,
            Tile tile,
            VectorTileFeature feature, IEnumerable<Layer> styleLayers, float scale, List<Rect> collisionBoxes,
            List<string> names)
        {
            Dictionary<string, string> attributes =
                feature.Attributes.ToDictionary(pair => pair.Key,
                    pair => pair.Value.ToString());

            var filterType = Convert(feature.GeometryType);

            if (filterType == null)
            {
                return;
            }

            var activeLayers = styleLayers.Where(styleLayer => styleLayer.Filter == null ||
                                                               styleLayer.Filter.Evaluate(filterType.Value, feature.Id,
                                                                   attributes)).ToList();

            var zoom = tile.ZoomLevel;

            var activeLayer = activeLayers.FirstOrDefault();

            if (activeLayer == null && feature.GeometryType != VectorTile.Tile.Types.GeomType.Point)
            {
                return;
            }

            var paint = activeLayer?.Paint;
            var color = paint?.FillColor?.GetValue(zoom);
            var fillColor = Convert(color) ?? Colors.Black;
            color = paint?.LineColor?.GetValue(zoom);
            var lineColor = Convert(color) ?? Colors.Black;

            if (paint?.LineOpacity != null)
            {
                var lineOpacity = paint.LineOpacity.GetValue(zoom);
                lineColor.A = (byte)(lineOpacity * byte.MaxValue);
            }

            color = paint?.TextColor?.GetValue(zoom);
            var lineWidth = (float?) paint?.LineWidth?.GetValue(zoom) ?? 1.0f;
            var textColor = Convert(color) ?? Colors.Black;

            var layout = activeLayer?.Layout;
            var fontSize = (float?) layout?.TextSize?.GetValue(zoom) ?? 16.0f;
            fontSize *= TileSize / 256;

            var fontFamily = layout?.TextFont?.FirstOrDefault();
            var textAnchor = layout?.TextAnchor ?? TextAnchor.Center;
            var textPadding = layout?.TextPadding?.GetValue(zoom) ?? 2.0;

            attributes.TryGetValue("name", out var name);

            switch (feature.GeometryType)
            {
                case VectorTile.Tile.Types.GeomType.Unknown:
                    break;
                case VectorTile.Tile.Types.GeomType.Point:
                    foreach (var point in feature.Geometry)
                    {
                        var anchor = point.First().ToVector2(scale);

                        if (name == null)
                        {
                            continue;
                        }

                        var format = new CanvasTextFormat
                        {
                            FontSize = fontSize,
                            WordWrapping = CanvasWordWrapping.NoWrap
                        };

                        if (fontFamily != null)
                        {
                            format.FontFamily = fontFamily;
                        }

                        switch (textAnchor)
                        {
                            case TextAnchor.Center:
                                format.HorizontalAlignment = CanvasHorizontalAlignment.Center;
                                format.VerticalAlignment = CanvasVerticalAlignment.Center;
                                break;
                            case TextAnchor.Left:
                                format.HorizontalAlignment = CanvasHorizontalAlignment.Left;
                                format.VerticalAlignment = CanvasVerticalAlignment.Center;
                                break;
                            case TextAnchor.Right:
                                format.HorizontalAlignment = CanvasHorizontalAlignment.Right;
                                format.VerticalAlignment = CanvasVerticalAlignment.Center;
                                break;
                            case TextAnchor.Top:
                                format.HorizontalAlignment = CanvasHorizontalAlignment.Center;
                                format.VerticalAlignment = CanvasVerticalAlignment.Top;
                                break;
                            case TextAnchor.Bottom:
                                format.HorizontalAlignment = CanvasHorizontalAlignment.Center;
                                format.VerticalAlignment = CanvasVerticalAlignment.Bottom;
                                break;
                            case TextAnchor.TopLeft:
                                format.HorizontalAlignment = CanvasHorizontalAlignment.Left;
                                format.VerticalAlignment = CanvasVerticalAlignment.Top;
                                break;
                            case TextAnchor.TopRight:
                                format.HorizontalAlignment = CanvasHorizontalAlignment.Right;
                                format.VerticalAlignment = CanvasVerticalAlignment.Top;
                                break;
                            case TextAnchor.BottomLeft:
                                format.HorizontalAlignment = CanvasHorizontalAlignment.Left;
                                format.VerticalAlignment = CanvasVerticalAlignment.Bottom;
                                break;
                            case TextAnchor.BottomRight:
                                format.HorizontalAlignment = CanvasHorizontalAlignment.Right;
                                format.VerticalAlignment = CanvasVerticalAlignment.Bottom;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        var layoutTest = new CanvasTextLayout(session, name, format, 0, 0);
                        var layoutBounds = layoutTest.DrawBounds;

                        double collisionX;
                        if (format.HorizontalAlignment == CanvasHorizontalAlignment.Center)
                        {
                            collisionX = anchor.X - layoutBounds.Width / 2;
                        }
                        else if (format.HorizontalAlignment == CanvasHorizontalAlignment.Right)
                        {
                            collisionX = anchor.X - layoutBounds.Width;
                        }
                        else
                        {
                            collisionX = anchor.X;
                        }

                        double collisionY;
                        if (format.VerticalAlignment == CanvasVerticalAlignment.Center)
                        {
                            collisionY = anchor.Y - layoutBounds.Height / 2;
                        }
                        else if (format.VerticalAlignment == CanvasVerticalAlignment.Bottom)
                        {
                            collisionY = anchor.Y - layoutBounds.Height;
                        }
                        else
                        {
                            collisionY = anchor.Y;
                        }

                        var collisionBox = new Rect(new Point(collisionX, collisionY),
                            new Size(layoutBounds.Width, layoutBounds.Height));

                        // TODO if something was drawn in one tile, draw it in others
                        if (collisionBoxes.All(box =>
                        {
                            box.Intersect(collisionBox);
                            return box.IsEmpty;
                        }))
                        {
                            session.DrawText(name, anchor, textColor, format);
                            collisionBoxes.Add(collisionBox);
                        }
                    }

                    break;

                case VectorTile.Tile.Types.GeomType.Linestring:
                case VectorTile.Tile.Types.GeomType.Polygon:
                {
                    var loop = feature.GeometryType == VectorTile.Tile.Types.GeomType.Polygon
                        ? CanvasFigureLoop.Closed
                        : CanvasFigureLoop.Open;

                    using (var pathBuilder = new CanvasPathBuilder(canvasRenderTarget))
                    {
                        foreach (var line in feature.Geometry)
                        {
                            var vectors = line.Select(p => p.ToVector2(scale)).ToList();

                            pathBuilder.BeginFigure(vectors[0]);

                            for (var i = 1; i < vectors.Count; i++)
                            {
                                pathBuilder.AddLine(vectors[i]);
                            }

                            pathBuilder.EndFigure(loop);
                        }

                        using (var geometry = CanvasGeometry.CreatePath(pathBuilder))
                        {
                            if (feature.GeometryType == VectorTile.Tile.Types.GeomType.Polygon)
                            {
                                session.FillGeometry(geometry, fillColor);
                            }
                            else
                            {
                                var strokeStyle = new CanvasStrokeStyle();

                                if (layout?.LineDashArray != null)
                                {
                                    strokeStyle.CustomDashStyle = layout.LineDashArray.ToArray();
                                }

                                if (layout?.LineCap != null)
                                {
                                    strokeStyle.StartCap = strokeStyle.EndCap = Convert(layout.LineCap.Value);
                                }

                                session.DrawGeometry(geometry, lineColor, lineWidth);


                                // "ref" is used for motorways
                                //attributes.TryGetValue("ref", out var refName);

                                if (name != null && !names.Contains(name))
                                {
                                        //var pathLength = geometry.ComputePathLength();
                                        //var textFormat = new CanvasTextFormat();
                                        //var textLayout = new CanvasTextLayout(session, name, textFormat, 0, 0);
                                        //var textWidth = textLayout.DrawBounds.Width;

                                        //var offset = (float) (pathLength - textWidth) / 2;

                                        //if (textWidth > pathLength)
                                        //{
                                        //    // for now just run away scared
                                        //    return;
                                        //}

                                        var format = new CanvasTextFormat
                                        {
                                            FontSize = fontSize,
                                            FontFamily = fontFamily,
                                            WordWrapping = CanvasWordWrapping.NoWrap
                                        };

                                        var line = feature.Geometry[0];
                                        var vectors = line.Select(p => p.ToVector2(scale)).ToList();

                                        var start = vectors[0];

                                        int charIndex = 0;
                                        foreach (var v in vectors.Skip(1))
                                        {
                                            if (DrawTextOnSegment(session, name, textColor, format, ref charIndex,
                                                ref start, v, false))
                                            {
                                                names.Add(name);
                                            }

                                            if (charIndex >= name.Length)
                                            {
                                                break;
                                            }
                                        }
                                    }
                            }
                        }
                    }
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static bool DrawTextOnSegment(CanvasDrawingSession gr, string txt, Color color, CanvasTextFormat format, ref int first_ch,
            ref Vector2 start_point, Vector2 end_point,
            bool text_above_segment)
        {
            float dx = end_point.X - start_point.X;
            float dy = end_point.Y - start_point.Y;
            float dist = Vector2.Distance(start_point, end_point);
            dx /= dist;
            dy /= dist;

            // See how many characters will fit.
            //int last_ch;
            //for (last_ch = txt.Length; last_ch > 0; last_ch--)
            //{
            //    string test_string =
            //        txt.Substring(first_ch, last_ch - first_ch);

            //    var textLayout = new CanvasTextLayout(gr, test_string, format, 0, 0);

            //    if (textLayout.DrawBounds.Width < dist)
            //    {
            //        break;
            //    }
            //}

            int last_ch = first_ch;
            while (last_ch < txt.Length)
            {
                string test_string =
                    txt.Substring(first_ch, last_ch - first_ch + 1);
                var textLayout = new CanvasTextLayout(gr, test_string, format, 0, 0);

                if (textLayout.DrawBounds.Width > dist)
                {
                    // This is one too many characters.
                    last_ch--;
                    break;
                }
                last_ch++;
            }

            if (last_ch < first_ch) return false;
            if (last_ch >= txt.Length) last_ch = txt.Length - 1;

            string chars_that_fit =
                txt.Substring(first_ch, last_ch - first_ch + 1);

            var transform = gr.Transform;

            var fitTextLayout = new CanvasTextLayout(gr, chars_that_fit, format, 0, 0);

            // Rotate and translate to position the characters.
            //GraphicsState state = gr.Save();
            //if (text_above_segment)
            //{
            //    gr.Transform = Matrix3x2.CreateTranslation(0, (float) fitTextLayout.DrawBounds.Height);
            //    //gr.TranslateTransform(0,
            //    //    -gr.MeasureString(chars_that_fit, font).Height,
            //    //    MatrixOrder.Append);
            //}


            float angle = (float) Math.Atan2(dy, dx);

            if (Math.Abs(angle) > Math.PI / 2)
            {
                return false;
            }

            var rotation = Matrix3x2.CreateRotation(angle, start_point);
            gr.Transform = Matrix3x2.Multiply(gr.Transform, rotation);

            //var translate = Matrix3x2.CreateTranslation(start_point.X, start_point.Y);
            //gr.Transform = Matrix3x2.Multiply(gr.Transform, translate);

            //gr.RotateTransform(angle, MatrixOrder.Append);
            //gr.TranslateTransform(start_point.X, start_point.Y,
            //    MatrixOrder.Append);

            // Draw the characters that fit.
            //gr.DrawString(chars_that_fit, font, brush, 0, 0);
            gr.DrawText(chars_that_fit, start_point, color, format);

            // Restore the saved state.
            //gr.Restore(state);
            gr.Transform = transform;

            // Update first_ch and start_point.
            first_ch = last_ch + 1;

            var text_width = (float) fitTextLayout.DrawBounds.Width;

            start_point = new Vector2(
                start_point.X + dx * text_width,
                start_point.Y + dy * text_width);

            return true;
        }

        private static Color? Convert(System.Drawing.Color? color)
        {
            if (color == null)
            {
                return null;
            }

            return Color.FromArgb(color.Value.A, color.Value.R, color.Value.G, color.Value.B);
        }

        private static FilterType? Convert(VectorTile.Tile.Types.GeomType type)
        {
            switch (type)
            {
                case VectorTile.Tile.Types.GeomType.Point:
                    return FilterType.Point;
                case VectorTile.Tile.Types.GeomType.Linestring:
                    return FilterType.LineString;
                case VectorTile.Tile.Types.GeomType.Polygon:
                    return FilterType.Polygon;
            }

            return null;
        }

        private static CanvasCapStyle Convert(LineCap lineCap)
        {
            switch (lineCap)
            {
                case LineCap.Butt:
                    return CanvasCapStyle.Flat;
                case LineCap.Round:
                    return CanvasCapStyle.Round;
                case LineCap.Square:
                    return CanvasCapStyle.Square;
                default:
                    return CanvasCapStyle.Flat;
            }
        }
    }
}