﻿using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Mapbox.Vector.Tile;
using MapboxStyle;
using MapControl;
using Microsoft.EntityFrameworkCore;
using MapTileLayer = MapControl.MapTileLayer;
using Tile = MapControl.Tile;

namespace MapTest
{
    public class DatabaseTileImageLoader : ITileImageLoader
    {
        private Style _style;

        public Task LoadTilesAsync(MapTileLayer tileLayer)
        {
            var pendingTiles = tileLayer.Tiles.Where(tile => tile.Pending);
            var tileLoadTasks = pendingTiles.Select(async tile => await LoadTileAsync(tile));
            return Task.WhenAll(tileLoadTasks);
        }

        private async Task LoadTileAsync(Tile tile)
        {
            using (var db = new DatabaseContext())
            {
                var column = tile.X;
                var row = (1 << tile.ZoomLevel) - tile.Y - 1;

                var tileModel = await db.Tiles.Include(model => model.Data).FirstOrDefaultAsync(model =>
                    model.ZoomLevel == tile.ZoomLevel && model.Column == column && model.Row == row);

                if (tileModel == null)
                {
                    return;
                }

                List<VectorTileLayer> layers;

                using (var dataStream = new MemoryStream(tileModel.Data.Data))
                using (var gzipStream = new GZipStream(dataStream, CompressionMode.Decompress))
                {
                    layers = VectorTileParser.Parse(gzipStream);
                }
                
                await TileRasterer.RasterAsync(tile, layers, _style);
            }
        }

        private void LoadStyle(double zoom)
        {
            if (_style != null)
            {
                return;
            }

            using (var stream = new FileStream("style.json", FileMode.Open, FileAccess.Read))
            {
                _style = StyleSerializer.Deserialize(stream);
            }
        }
    }
}
