using System.Collections.Generic;
using System.Linq;

namespace Mapbox.Vector.Tile
{
    public static class AttributesParser
    {
        public static List<KeyValuePair<string, object>> Parse(List<string> keys, List<VectorTile.Tile.Types.Value> values, List<uint> tags)
        {
            var result = new List<KeyValuePair<string, object>>();
            var odds = tags.GetOdds().ToList();
            var evens = tags.GetEvens().ToList();

            for (var i = 0; i < evens.Count; i++)
            {
                var key = keys[(int)evens[i]];
                var val = values[(int)odds[i]];
                var valObject = GetAttr(val);
                result.Add(new KeyValuePair<string, object>(key, valObject));
            }
            return result;
        }

        private static object GetAttr(VectorTile.Tile.Types.Value value)
        {
            switch (value.ValueOneofCase)
            {
                case VectorTile.Tile.Types.Value.ValueOneofOneofCase.BoolValue:
                    return value.BoolValue;
                case VectorTile.Tile.Types.Value.ValueOneofOneofCase.DoubleValue:
                    return value.DoubleValue;
                case VectorTile.Tile.Types.Value.ValueOneofOneofCase.FloatValue:
                    return value.FloatValue;
                case VectorTile.Tile.Types.Value.ValueOneofOneofCase.IntValue:
                    return value.IntValue;
                case VectorTile.Tile.Types.Value.ValueOneofOneofCase.SintValue:
                    return value.SintValue;
                case VectorTile.Tile.Types.Value.ValueOneofOneofCase.UintValue:
                    return value.UintValue;
                case VectorTile.Tile.Types.Value.ValueOneofOneofCase.StringValue:
                    return value.StringValue;
                default:
                    return null;
            }
        }
    }
}
