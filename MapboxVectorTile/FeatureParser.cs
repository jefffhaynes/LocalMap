using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Mapbox.Vector.Tile
{
    public static class FeatureParser
    {

        public static VectorTileFeature Parse(VectorTile.Tile.Types.Feature feature, List<string> keys, List<VectorTile.Tile.Types.Value> values,uint extent)
        {
            var result = new VectorTileFeature();
            var id = feature.Id;

            var geom =  GeometryParser.ParseGeometry(feature.Geometry.ToList(), feature.Type);
            result.GeometryType = feature.Type;

            // add the geometry
            result.Geometry = geom;
            result.Extent = extent;

            // now add the attributes
            result.Id = id.ToString(CultureInfo.InvariantCulture);
            result.Attributes = AttributesParser.Parse(keys, values, feature.Tags.ToList());
            return result;
        }
    }
}
