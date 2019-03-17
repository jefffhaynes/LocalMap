using System.Linq;
using BAMCIS.GeoJSON;

namespace Mapbox.Vector.Tile
{
	public static class VectorTileLayerExtensions
	{
		public static FeatureCollection ToGeoJson(this VectorTileLayer vectorTileLayer, int x, int y, int z)
        {
            var geoJsonCollection = vectorTileLayer.VectorTileFeatures.Select(f => f.ToGeoJson(x, y, z))
                .Where(f => f.Geometry != null);
            return new FeatureCollection(geoJsonCollection);
		}
	}
}

