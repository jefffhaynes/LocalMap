using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Mapbox.Vector.Tile;
using MapboxStyle;
using LocalMap;
using Microsoft.EntityFrameworkCore;

namespace LocalMap
{
    public static class DatabaseTileImageLoader
    {
        private static Style _style;

        public static async Task<IRandomAccessStream> LoadTileAsync(Tile tile)
        {
            LoadStyle();

            //tile = new Tile(8529, 5974, 14);

            const int maxZoomLevel = 14;

            using (var db = new DatabaseContext())
            {
                Tile effectiveTile;
                int overzoom = 0;
                Vector2 overzoomOffset = Vector2.Zero;
                if (tile.ZoomLevel > maxZoomLevel)
                {
                    overzoom = (int) Math.Pow(2, tile.ZoomLevel - maxZoomLevel);
                    overzoomOffset = new Vector2(tile.X % overzoom, tile.Y % overzoom);
                    effectiveTile = new Tile(tile.X / overzoom, tile.Y / overzoom, maxZoomLevel);
                }
                else
                {
                    effectiveTile = tile;
                }

                var column = effectiveTile.X;
                var row = (1 << effectiveTile.ZoomLevel) - effectiveTile.Y - 1;

                var tileModel = await db.Tiles.Include(model => model.Data).FirstOrDefaultAsync(model =>
                    model.ZoomLevel == effectiveTile.ZoomLevel && model.Column == column && model.Row == row);

                if (tileModel == null)
                {
                    return null;
                }

                List<VectorTileLayer> layers;

                using (var dataStream = new MemoryStream(tileModel.Data.Data))
                using (var gzipStream = new GZipStream(dataStream, CompressionMode.Decompress))
                {
                    layers = VectorTileParser.Parse(gzipStream);
                }

                //if (overzoom > 0)
                //{
                //    var tileOffset = overzoomOffset / overzoom;
                //    var size = Vector2.One / overzoom;

                //    foreach (var vectorTileLayer in layers)
                //    {
                //        vectorTileLayer.Clip(new RectangleF(tileOffset.X, tileOffset.Y, size.X, size.Y));
                //    }

                //    if (layers.Count == 0)
                //    {
                //        return null;
                //    }
                //}

                var tileOffset = overzoom > 0 ? overzoomOffset / overzoom + Vector2.One / (overzoom * 2) : Vector2.Zero;
                return await TileRasterer.RasterAsync(layers, _style, effectiveTile.ZoomLevel, overzoom, tileOffset);
            }
        }

        private static readonly object StyleLock = new object();

        private static void LoadStyle()
        {
            if (_style != null)
            {
                return;
            }

            lock (StyleLock)
            {
                if (_style != null)
                {
                    return;
                }

                var assembly = typeof(DatabaseTileImageLoader).Assembly;

                var resourceName = $"{assembly.GetName().Name}.style.json";
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    _style = StyleSerializer.Deserialize(stream);
                }
            }
        }
    }
}
