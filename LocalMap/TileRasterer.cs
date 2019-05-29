using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
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

namespace LocalMap
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
            Tile tile, VectorTileFeature feature, IEnumerable<Layer> styleLayers, float scale,
            List<Rect> collisionBoxes,
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

            if (paint?.FillOpacity != null)
            {
                var fillOpacity = paint.FillOpacity.GetValue(zoom);
                fillColor.A = (byte) (fillOpacity * byte.MaxValue);
            }

            var lineColor = Convert(color) ?? Colors.Black;

            if (paint?.LineOpacity != null)
            {
                var lineOpacity = paint.LineOpacity.GetValue(zoom);
                lineColor.A = (byte) (lineOpacity * byte.MaxValue);
            }

            color = paint?.TextColor?.GetValue(zoom);
            var lineWidth = (float?) paint?.LineWidth?.GetValue(zoom) ?? 1.0f;
            var textColor = Convert(color) ?? Colors.Black;

            color = paint?.TextHaloColor?.GetValue(zoom);
            var textHaloColor = Convert(color);
            var textHaloWidth = (float?) paint?.TextHaloWidth?.GetValue(zoom) ?? 0.0f;

            var layout = activeLayer?.Layout;
            var fontSize = (float?) layout?.TextSize?.GetValue(zoom) ?? 16.0f;
            fontSize *= TileSize / 256;

            var fontFamily = layout?.TextFont?.FirstOrDefault();
            var textPadding = layout?.TextPadding?.GetValue(zoom) ?? 2.0;

            if (attributes.TryGetValue("name", out var name))
            {
                var textTransform = layout?.TextTransform ?? TextTransform.None;

                switch (textTransform)
                {
                    case TextTransform.Lowercase:
                        name = name.ToLowerInvariant();
                        break;
                    case TextTransform.Uppercase:
                        name = name.ToUpperInvariant();
                        break;
                }
            }

            switch (feature.GeometryType)
            {
                case VectorTile.Tile.Types.GeomType.Point:
                {
                    var point = feature.Geometry[0][0];
                    var anchor = point.ToVector2(scale);

                    var textAnchor = layout?.TextAnchor ?? TextAnchor.Center;
                    var maximumTextWidth = (float?) layout?.MaximumTextWidth?.GetValue(zoom) ?? 10.0f;

                    DrawLabel(session, name, anchor, fontSize, fontFamily, textColor, textAnchor, maximumTextWidth,
                        textPadding, textHaloColor, textHaloWidth, collisionBoxes);
                    break;
                }

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
                            using (var strokeStyle = new CanvasStrokeStyle())
                            {
                                if (paint?.LineDashArray != null)
                                {
                                    strokeStyle.CustomDashStyle = paint.LineDashArray.Select(a => a / scale).ToArray();
                                }

                                if (layout?.LineCap != null)
                                {
                                    strokeStyle.StartCap = strokeStyle.EndCap = Convert(layout.LineCap.Value);
                                }

                                if (layout?.LineJoin != null)
                                {
                                    strokeStyle.LineJoin = Convert(layout.LineJoin.Value);
                                }

                                if (feature.GeometryType == VectorTile.Tile.Types.GeomType.Polygon)
                                {
                                    session.FillGeometry(geometry, fillColor);
                                    var fillOutlineColor = Convert(paint?.FillOutlineColor?.GetValue(zoom));

                                    if (fillOutlineColor != null)
                                    {
                                        session.DrawGeometry(geometry, fillOutlineColor.Value, 1.0f / scale,
                                            strokeStyle);
                                    }
                                }
                                else
                                {
                                    session.DrawGeometry(geometry, lineColor, lineWidth, strokeStyle);


                                    // "ref" is used for motorways
                                    //attributes.TryGetValue("ref", out var refName);

                                    if (name != null && !names.Contains(name))
                                    {
                                        using (var format = new CanvasTextFormat {FontSize = fontSize, WordWrapping = CanvasWordWrapping.NoWrap})
                                        {
                                            if (fontFamily != null)
                                            {
                                                format.FontFamily = fontFamily;
                                            }

                                            var line = feature.Geometry[0];
                                            var vectors = line.Select(p => p.ToVector2(scale)).ToList();

                                            if (session.DrawTextOnSegments(name, vectors, textColor, format))
                                            {
                                                names.Add(name);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                    break;
            }
        }

        private static void DrawLabel(CanvasDrawingSession session, string name, Vector2 anchor, float fontSize,
            string fontFamily, Color textColor, TextAnchor textAnchor, float maximumTextWidth, double textPadding,
            Color? textHaloColor, float textHaloWidth, List<Rect> collisionBoxes)
        {
            if (name == null)
            {
                return;
            }

            using (var format = new CanvasTextFormat
            {
                FontSize = fontSize,
                WordWrapping = CanvasWordWrapping.NoWrap
            })
            {
                if (fontFamily != null)
                {
                    format.FontFamily = fontFamily;
                }

                Convert(textAnchor, out var horizontalAlignment, out var verticalAlignment);
                format.HorizontalAlignment = horizontalAlignment;
                //format.VerticalAlignment = verticalAlignment;

                var wrappingWidth = GetWrappingWidth(session, name, format, maximumTextWidth * fontSize);
                format.WordWrapping = CanvasWordWrapping.WholeWord;

                using (var layout = new CanvasTextLayout(session, name, format, wrappingWidth, 0))
                {
                    var layoutBounds = layout.LayoutBounds;

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

                    var collisionPoint = new Point(collisionX, collisionY);
                    var collisionSize = new Size(layoutBounds.Width, layoutBounds.Height);
                    var collisionBox = new Rect(collisionPoint, collisionSize).Expand(textPadding);

                    var clippingRect = new Rect(0, 0, TileSize, TileSize);
                    if (!clippingRect.Contains(collisionBox))
                    {
                        return;
                    }

                    if (collisionBoxes.All(box =>
                    {
                        box.Intersect(collisionBox);
                        return box.IsEmpty;
                    }))
                    {
                        if (textHaloColor != null)
                        {
                            var textGeometry = CanvasGeometry.CreateText(layout);
                            session.DrawGeometry(textGeometry, collisionPoint.ToVector2(), textHaloColor.Value, textHaloWidth * 2);
                        }

                        session.DrawTextLayout(layout, collisionPoint.ToVector2(), textColor);
                        //session.DrawRectangle(collisionBox, Colors.Purple);
                        collisionBoxes.Add(collisionBox);
                    }
                }
            }
        }

        private static float GetWrappingWidth(CanvasDrawingSession session, string name, CanvasTextFormat format, float maxWidth)
        {
            var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var wordLengths = words.Select(word => Measure(session, word, format)).ToList();

            if (wordLengths.Count == 1)
            {
                return wordLengths.First();
            }

            var spaceLength = Measure(session, " ", format);

            float maxRowWidth = 0;
            float rowWidth = 0;
            foreach (var wordLength in wordLengths)
            {
                if (rowWidth + wordLength > maxWidth)
                {
                    // no more words will fit on this row.  next row
                    maxRowWidth = Math.Max(maxRowWidth, rowWidth);
                    rowWidth = wordLength;
                }
                else if (rowWidth > 0)
                {
                    // more words fit and we already have at least one word, so add a space
                    rowWidth += spaceLength + wordLength;
                }
                else
                {
                    // first word
                    rowWidth = wordLength;
                }
            }

            // add one b/c we need to make sure layout happens inside this
            return Math.Max(maxRowWidth, rowWidth) + 1; 
        }

        private static float Measure(CanvasDrawingSession session, string value, CanvasTextFormat format)
        {
            using (var layout = new CanvasTextLayout(session, value, format, 0, 0))
            {
                return (float) layout.LayoutBoundsIncludingTrailingWhitespace.Width;
            }
        }

        private static void Convert(TextAnchor textAnchor, out CanvasHorizontalAlignment horizontalAlignment, out CanvasVerticalAlignment verticalAlignment)
        {
            switch (textAnchor)
            {
                case TextAnchor.Center:
                    horizontalAlignment = CanvasHorizontalAlignment.Center;
                    verticalAlignment = CanvasVerticalAlignment.Center;
                    break;
                case TextAnchor.Left:
                    horizontalAlignment = CanvasHorizontalAlignment.Left;
                    verticalAlignment = CanvasVerticalAlignment.Center;
                    break;
                case TextAnchor.Right:
                    horizontalAlignment = CanvasHorizontalAlignment.Right;
                    verticalAlignment = CanvasVerticalAlignment.Center;
                    break;
                case TextAnchor.Top:
                    horizontalAlignment = CanvasHorizontalAlignment.Center;
                    verticalAlignment = CanvasVerticalAlignment.Top;
                    break;
                case TextAnchor.Bottom:
                    horizontalAlignment = CanvasHorizontalAlignment.Center;
                    verticalAlignment = CanvasVerticalAlignment.Bottom;
                    break;
                case TextAnchor.TopLeft:
                    horizontalAlignment = CanvasHorizontalAlignment.Left;
                    verticalAlignment = CanvasVerticalAlignment.Top;
                    break;
                case TextAnchor.TopRight:
                    horizontalAlignment = CanvasHorizontalAlignment.Right;
                    verticalAlignment = CanvasVerticalAlignment.Top;
                    break;
                case TextAnchor.BottomLeft:
                    horizontalAlignment = CanvasHorizontalAlignment.Left;
                    verticalAlignment = CanvasVerticalAlignment.Bottom;
                    break;
                case TextAnchor.BottomRight:
                    horizontalAlignment = CanvasHorizontalAlignment.Right;
                    verticalAlignment = CanvasVerticalAlignment.Bottom;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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

        private static CanvasLineJoin Convert(LineJoin lineJoin)
        {
            switch (lineJoin)
            {
                case LineJoin.Bevel:
                    return CanvasLineJoin.Bevel;
                case LineJoin.Round:
                    return CanvasLineJoin.Round;
                case LineJoin.Miter:
                    return CanvasLineJoin.Miter;
                default:
                    return default;
            }
        }
    }
}