using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using LocalMap.Models;
using Mapbox.Vector.Tile;
using MapboxStyle;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graphics.Canvas;

namespace LocalMap
{
    public static class DatabaseTileImageLoader
    {
        private static Style _style;

        public static async Task<IRandomAccessStream> LoadTileAsync(Tile tile)
        {
            LoadStyle();

            var layers = await GetLayersAsync(tile);

            return await TileRasterer.RasterAsync(layers, _style, tile.ZoomLevel);
        }

        public static async Task DrawTileAsync(Tile tile, CanvasDrawingSession session, int tileSize)
        {
            LoadStyle();

            var layers = await GetLayersAsync(tile);

            TileRasterer.Raster(session, layers, _style, tile.ZoomLevel, tileSize);
        }

        private static async Task<List<VectorTileLayer>> GetLayersAsync(Tile tile)
        {
            var tileModel = await GetTileModelAsync(tile);

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

            return layers;
        }

        private static async Task<TileModel> GetTileModelAsync(Tile tile)
        {
            using (var db = new DatabaseContext())
            {
                var column = tile.X;
                var row = (1 << tile.ZoomLevel) - tile.Y - 1;

                var tileModel = await db.Tiles.Include(model => model.Data).FirstOrDefaultAsync(model =>
                    model.ZoomLevel == tile.ZoomLevel && model.Column == column && model.Row == row);
                return tileModel;
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
