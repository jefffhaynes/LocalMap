using System;
using System.IO;
using BruTile;
using Mapsui;

namespace LocalMap
{
    public class VectorTileSource : ITileSource
    {
        public VectorTileSource(ITileSchema schema)
        {
            Schema = schema;
        }

        public ITileSchema Schema { get; }

        public string Name => throw new NotImplementedException();

        public Attribution Attribution => new Attribution();

        public byte[] GetTile(TileInfo tileInfo)
        {
            var index = tileInfo.Index;
            var streamTask = DatabaseTileImageLoader.LoadTileAsync(new Tile(index.Col, index.Row, int.Parse(index.Level)));
            streamTask.Wait();
            var stream = streamTask.Result;

            return stream?.AsStream().ToBytes();
        }
    }
}