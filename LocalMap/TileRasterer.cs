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
            var textAnchor = layout?.TextAnchor ?? TextAnchor.Center;
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
                    DrawLabel(session, name, feature.Geometry, scale, fontSize, fontFamily, textColor, textAnchor,
                        textPadding, textHaloColor, textHaloWidth, collisionBoxes);
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
                                var fillOutlineColor = Convert(paint?.FillOutlineColor?.GetValue(zoom));

                                if (fillOutlineColor != null)
                                {
                                    session.DrawGeometry(geometry, Vector2.Zero, fillOutlineColor.Value);
                                }
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

                                if (layout?.LineJoin != null)
                                {
                                    strokeStyle.LineJoin = Convert(layout.LineJoin.Value);
                                }

                                session.DrawGeometry(geometry, lineColor, lineWidth, strokeStyle);


                                // "ref" is used for motorways
                                //attributes.TryGetValue("ref", out var refName);

                                if (name != null && !names.Contains(name))
                                {
                                    var format = new CanvasTextFormat
                                    {
                                        FontSize = fontSize,
                                        FontFamily = fontFamily,
                                        WordWrapping = CanvasWordWrapping.NoWrap
                                    };

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
                    break;
                case VectorTile.Tile.Types.GeomType.Unknown:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void DrawLabel(CanvasDrawingSession session, string name, List<List<Coordinate>> geometry,
            float scale, float fontSize,
            string fontFamily, Color textColor, TextAnchor textAnchor, double textPadding, Color? textHaloColor,
            float textHaloWidth,
            List<Rect> collisionBoxes)
        {
            if (name == null)
            {
                return;
            }

            foreach (var point in geometry)
            {
                var anchor = point.First().ToVector2(scale);

                var format = new CanvasTextFormat
                {
                    FontSize = fontSize,
                    WordWrapping = CanvasWordWrapping.NoWrap
                };

                if (fontFamily != null)
                {
                    format.FontFamily = fontFamily;
                }

                Convert(textAnchor, out var horizontalAlignment, out var verticalAlignment);
                format.HorizontalAlignment = horizontalAlignment;
                format.VerticalAlignment = verticalAlignment;

                var layout = new CanvasTextLayout(session, name, format, 0, 0);
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

                var collisionBox = new Rect(new Point(collisionX, collisionY),
                    new Size(layoutBounds.Width, layoutBounds.Height)).Expand(textPadding);

                // TODO if something was drawn in one tile, draw it in others at the same zoom level
                if (collisionBoxes.All(box =>
                {
                    box.Intersect(collisionBox);
                    return box.IsEmpty;
                }))
                {
                    if (textHaloColor != null)
                    {
                        var textGeometry = CanvasGeometry.CreateText(layout);
                        session.DrawGeometry(textGeometry, Vector2.Zero, textHaloColor.Value, textHaloWidth * 2);
                    }

                    session.DrawText(name, anchor, textColor, format);
                    collisionBoxes.Add(collisionBox);
                }
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