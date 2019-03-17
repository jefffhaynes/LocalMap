﻿using System.Collections.Generic;

namespace Mapbox.Vector.Tile
{
	public class VectorTileFeature
	{
        public string Id { get; set; }
		public List<List<Coordinate>> Geometry {get;set;}
		public List<KeyValuePair<string, object>> Attributes { get; set; }
		public VectorTile.Tile.Types.GeomType GeometryType { get; set; }
        public uint Extent { get; set; }
    }
}

