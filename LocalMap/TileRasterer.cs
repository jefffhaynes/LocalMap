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
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;

namespace MapTest
{
    public static class TileRasterer
    {
        private static readonly Dictionary<string, Color> LayerColors = new Dictionary<string, Color>
        {
            {"water", Color.FromArgb(Byte.MaxValue, 148, 193, 225)},
            {"water_name", Colors.Black},
            {"waterway", Color.FromArgb(Byte.MaxValue, 148, 193, 225)}
        };

        private static readonly Dictionary<string, Color> FeatureColors = new Dictionary<string, Color>
        {
            {"residential", Color.FromArgb(178, 224, 222, 215)},
            {"trunk", Colors.SaddleBrown},
            {"primary", Colors.SaddleBrown},
            {"wood", Color.FromArgb(Byte.MaxValue, 192, 216, 151)},
            {"grass", Colors.YellowGreen},
            {"farmland", Colors.Goldenrod},
            {"motorway", Colors.Brown},
            {"state", Colors.Orange},
            {"continent", Colors.DarkKhaki},
            {"country", Colors.Black},
            {"national_park", Colors.LightGreen},
            {"nature_reserve", Colors.LightGreen},
            {"park", Colors.LightGreen},
            {"ice", Colors.GhostWhite},
            {"city", Colors.Gray},
            {"town", Colors.Gray},
            {"village", Colors.Gray},
            {"hamlet", Colors.Gray},
            {"suburb", Colors.Gray}
        };


        public static async Task RasterAsync(Tile tile, List<VectorTileLayer> layers, Style style)
        {
            using (var canvasDevice = new CanvasDevice())
            {
                const float size = 512;
                using (var canvasRenderTarget = new CanvasRenderTarget(canvasDevice, size, size, 96))
                {
                    using (var session = canvasRenderTarget.CreateDrawingSession())
                    {
                        session.Antialiasing = CanvasAntialiasing.Antialiased;
                        session.TextAntialiasing = CanvasTextAntialiasing.ClearType;
                        session.FillRectangle(new Rect(0, 0, size, size), Colors.White);

                        foreach (var layer in layers)
                        {
                            var scale = size / layer.Extent;

                            foreach (var feature in layer.VectorTileFeatures)
                            {
                                var attributes =
                                    feature.Attributes.ToDictionary(pair => pair.Key,
                                        pair => pair.Value.ToString());

                                Color color;

                                if (attributes.TryGetValue("class", out var featureClass))
                                {
                                    if (!FeatureColors.TryGetValue(featureClass, out color))
                                    {
                                        if (!LayerColors.TryGetValue(layer.Name, out color))
                                        {
                                            continue;
                                        }
                                    }
                                }
                                else
                                {
                                    continue;
                                }

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
                                                session.DrawText(name, center, color,
                                                    new CanvasTextFormat
                                                    {
                                                        HorizontalAlignment = CanvasHorizontalAlignment.Center,
                                                        FontSize = 9
                                                    });
                                            }
                                        }

                                        break;

                                    case VectorTile.Tile.Types.GeomType.Linestring:
                                    case VectorTile.Tile.Types.GeomType.Polygon:
                                        {
                                            foreach (var line in feature.Geometry)
                                            {
                                                using (var pathBuilder = new CanvasPathBuilder(canvasRenderTarget))
                                                {
                                                    Coordinate first = line.First();
                                                    Vector2 p1 = first.ToVector2(scale);
                                                    pathBuilder.BeginFigure(p1);

                                                    foreach (var coordinate in line.Skip(1))
                                                    {
                                                        pathBuilder.AddLine(coordinate.ToVector2(scale));
                                                    }

                                                    var loop = feature.GeometryType == VectorTile.Tile.Types.GeomType.Polygon
                                                        ? CanvasFigureLoop.Closed
                                                        : CanvasFigureLoop.Open;

                                                    pathBuilder.EndFigure(loop);

                                                    using (var geometry = CanvasGeometry.CreatePath(pathBuilder))
                                                    {
                                                        if (feature.GeometryType == VectorTile.Tile.Types.GeomType.Polygon)
                                                        {
                                                            session.FillGeometry(geometry, color);
                                                        }
                                                        else
                                                        {
                                                            session.DrawGeometry(geometry, color);
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
                        }
                    }

                    var image = new BitmapImage();

                    var stream = new InMemoryRandomAccessStream();

                    await canvasRenderTarget.SaveAsync(stream, CanvasBitmapFileFormat.Png);
                    image.SetSource(stream);

                    tile.SetImage(image);
                }
            }
        }
    }
}
