using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

        //public async Task LoadTilesAsync(MapTileLayer tileLayer)
        //{
        //    var pendingTiles = tileLayer.Tiles.Where(tile => tile.Pending);

        //    foreach (var pendingTile in pendingTiles)
        //    {
        //        await LoadTileAsync(pendingTile);
        //    }

        //    //var tileLoadTasks = pendingTiles.Select(async tile => await LoadTileAsync(tile));
        //    //return Task.WhenAll(tileLoadTasks);
        //}

        public static async Task<IRandomAccessStream> LoadTileAsync(Tile tile)
        {
            LoadStyle();

            using (var db = new DatabaseContext())
            {
                var column = tile.X;
                var row = (1 << tile.ZoomLevel) - tile.Y - 1;

                var tileModel = await db.Tiles.Include(model => model.Data).FirstOrDefaultAsync(model =>
                    model.ZoomLevel == tile.ZoomLevel && model.Column == column && model.Row == row);

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
                
                return await TileRasterer.RasterAsync(tile, layers, _style);
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
