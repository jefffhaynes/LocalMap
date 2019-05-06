using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Mapbox.Vector.Tile;
using MapboxStyle;
using MapControl;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;

namespace MapTest
{
    public static class TileRasterer
    {
        private static readonly MemoryCache Cache = new MemoryCache(new MemoryCacheOptions());

        public static async Task RasterAsync(Tile tile, List<VectorTileLayer> layers, Style style)
        {
            var imageKey = new {tile.X, tile.Y, tile.ZoomLevel};

            if (!Cache.TryGetValue(imageKey, out BitmapImage image))
            {
                using (var canvasDevice = new CanvasDevice())
                {
                    const float size = 1024;
                    using (var canvasRenderTarget = new CanvasRenderTarget(canvasDevice, size, size, 96))
                    {
                        using (var session = canvasRenderTarget.CreateDrawingSession())
                        {
                            session.Antialiasing = CanvasAntialiasing.Antialiased;
                            session.TextAntialiasing = CanvasTextAntialiasing.ClearType;
                            session.FillRectangle(new Rect(0, 0, size, size), Colors.White);

                            foreach (var layer in layers)
                            {
                                RasterLayer(session, canvasRenderTarget, tile, layer, style, size);
                            }
                        }

                        image = new BitmapImage();

                        var stream = new InMemoryRandomAccessStream();

                        await canvasRenderTarget.SaveAsync(stream, CanvasBitmapFileFormat.Png);
                        image.SetSource(stream);

                        Cache.Set(imageKey, image);
                    }
                }
            }
            
            tile.SetImage(image);
        }

        private static void RasterLayer(CanvasDrawingSession session, CanvasRenderTarget canvasRenderTarget, Tile tile,
            VectorTileLayer layer, Style style, float size)
        {
            var styleLayers = style.GetLayers(layer.Name, tile.ZoomLevel);

            var scale = size / layer.Extent;

            foreach (var feature in layer.VectorTileFeatures)
            {
                RasterFeature(session, canvasRenderTarget, tile, feature, styleLayers, scale);
            }
        }

        private static void RasterFeature(CanvasDrawingSession session, CanvasRenderTarget canvasRenderTarget,
            Tile tile,
            VectorTileFeature feature, IEnumerable<Layer> styleLayers, float scale)
        {
            var attributes =
                feature.Attributes.ToDictionary(pair => pair.Key,
                    pair => pair.Value.ToString());

            var filterType = Convert(feature.GeometryType).Value;
            var activeLayers = styleLayers.Where(styleLayer => styleLayer.Filter == null ||
                                                               styleLayer.Filter.Evaluate(filterType, feature.Id,
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
            color = paint?.TextColor?.GetValue(zoom);
            var lineWidth = (float?) paint?.LineWidth?.GetValue(zoom) ?? 1.0f;
            var textColor = Convert(color) ?? Colors.Black;

            var layout = activeLayer?.Layout;
            var fontSize = (float?) layout?.TextSize?.GetValue(zoom) ?? 16.0f;
            var fontFamily = layout?.TextFont?.FirstOrDefault();

            attributes.TryGetValue("name", out var name);

            switch (feature.GeometryType)
            {
                case VectorTile.Tile.Types.GeomType.Unknown:
                    break;
                case VectorTile.Tile.Types.GeomType.Point:
                    foreach (var point in feature.Geometry)
                    {
                        var center = point.First().ToVector2(scale);

                        if (name != null)
                        {
                            var format = new CanvasTextFormat
                            {
                                HorizontalAlignment = CanvasHorizontalAlignment.Center,
                                FontSize = fontSize
                            };

                            if (fontFamily != null)
                            {
                                format.FontFamily = fontFamily;
                            }

                            session.DrawText(name, center, textColor, format);
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
                        Vector2 first = Vector2.Zero;
                        Vector2 last = Vector2.Zero;

                        foreach (var line in feature.Geometry)
                        {
                            var vectors = line.Select(p => p.ToVector2(scale)).ToList();

                            first = vectors[0];
                            last = vectors[1];

                            pathBuilder.BeginFigure(first);

                            for (int i = 1; i < vectors.Count; i++)
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

                                if (name != null)
                                {
                                    var v = first - last;
                                    var angle = Math.Atan2(v.Y, v.X);

                                    session.Transform = Matrix3x2.CreateRotation((float) angle);

                                    var center = (first + last) / 2;
                                    session.DrawText(name, center, textColor);

                                    session.Transform = Matrix3x2.Identity;
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
