using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mapbox.Vector.Tile
{
    public static class VectorTileParser
    {
        public static List<VectorTileLayer> Parse(Stream stream)
        {
            var tile = VectorTile.Tile.Parser.ParseFrom(stream);
            var list = new List<VectorTileLayer>();
            foreach (var layer in tile.Layers)
            {
                var extent = layer.Extent;
                var vectorTileLayer = new VectorTileLayer
                {
                    Name = layer.Name, Version = layer.Version, Extent = layer.Extent
                };

                foreach (VectorTile.Tile.Types.Feature feature in layer.Features)
                {
                    var vectorTileFeature = FeatureParser.Parse(feature, layer.Keys.ToList(), layer.Values.ToList(), extent);
                    vectorTileLayer.VectorTileFeatures.Add(vectorTileFeature);
                }

                list.Add(vectorTileLayer);
            }
            return list;
        }
    }
}
