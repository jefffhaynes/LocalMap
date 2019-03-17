using System.Collections.Generic;
using System.Linq;
using BAMCIS.GeoJSON;

namespace Mapbox.Vector.Tile
{
    public static class VectorTileFeatureExtensions
    {
        
        private static List<Position> Project(List<Coordinate> coords, int x, int y, int z, uint extent)
        {
            return coords.Select(coordinate => coordinate.ToPosition(x, y, z, extent)).ToList();
        }

        private static LineString CreateLineString(List<Position> pos)
        {
            return new LineString(pos);
        }


        private static Geometry GetPointGeometry(List<Position> pointList)
        {
            if (pointList.Count == 1)
            {
                return new Point(pointList[0]);
            }

            var positions = pointList.Select(position => new Position(position.Longitude, position.Latitude, position.Elevation)).ToList();
            return new MultiPoint(positions);
        }

        private static List<LineString> GetLineStringList(List<List<Position>> pointList)
        {
            return pointList.Select(CreateLineString).ToList();
        }

        private static Geometry GetLineGeometry(List<List<Position>> pointList)
        {
            Geometry geom;

            if (pointList.Count == 1)
            {
                geom = new LineString(pointList[0]);
            }
            else
            {
                geom = new MultiLineString(GetLineStringList(pointList));
            }
            return geom;
        }

        private static Polygon GetPolygon(List<List<Position>> lines)
        {
            var res = new List<LinearRing>();
            foreach (var innerRing in lines)
            {
                if (innerRing.First() == innerRing.Last())
                {
                    res.Add(new LinearRing(innerRing));
                }
            }

            var geom = new Polygon(res);
            return geom;
        }

        private static Geometry GetPolygonGeometry(List<List<List<Position>>> polygons)
        {
            {
                Geometry geom=null;

                if (polygons.Count == 1)
                {
                    geom = GetPolygon(polygons[0]);
                }
                else if(polygons.Count>0)
                {
                    var coordinates = new List<Polygon>();
                    foreach(var poly in polygons)
                    {
                        var pl = GetPolygon(poly);
                        coordinates.Add(pl);
                    }

                    geom = new MultiPolygon(coordinates);
                }
                return geom;
            }
        }

        public static List<Position> ProjectPoints(List<List<Coordinate>> geometry, int x, int y, int z, uint extent)
        {
            var projectedCoords = new List<Position>();
            var coords = new List<Coordinate>();

            foreach (var g in geometry)
            {
                coords.Add(g[0]);
                projectedCoords = Project(coords, x, y, z, extent);
            }
            return projectedCoords;
        }

        public static List<List<Position>> ProjectLines(List<List<Coordinate>> geometry, int x, int y, int z, uint extent)
        {
            var pointList = new List<List<Position>>();
            foreach (var g in geometry)
            {
                var projectedCoords = Project(g, x, y, z, extent);
                pointList.Add(projectedCoords);
            }
            return pointList;
        }

        public static List<List<List<Position>>> ProjectPolygons(List<List<List<Coordinate>>> geometry, int x, int y, int z, uint extent)
        {
            var result = new List<List<List<Position>>>();
            foreach (var g in geometry)
            {
                var projectedCoords = ProjectLines(g, x, y, z, extent);
                result.Add(projectedCoords);
            }
            return result;
        }

        public static Feature ToGeoJson(this VectorTileFeature vectorTileFeature, int x, int y, int z)
        {
            Geometry geom = null;

            switch (vectorTileFeature.GeometryType)
            {
                case VectorTile.Tile.Types.GeomType.Point:
                    var projectedPoints = ProjectPoints(vectorTileFeature.Geometry, x, y, z, vectorTileFeature.Extent);
                    geom = GetPointGeometry(projectedPoints);
                    break;
                case VectorTile.Tile.Types.GeomType.Linestring:
                    var projectedLines = ProjectLines(vectorTileFeature.Geometry, x, y, z, vectorTileFeature.Extent);
                    geom = GetLineGeometry(projectedLines);
                    break;
                case VectorTile.Tile.Types.GeomType.Polygon:
                    var rings = ClassifyRings.Classify(vectorTileFeature.Geometry);
                    var projectedPolygons = ProjectPolygons(rings, x, y, z, vectorTileFeature.Extent);
                    geom = GetPolygonGeometry(projectedPolygons);
                    break;
            }

            var attributes = vectorTileFeature.Attributes.ToDictionary(pair => pair.Key, pair => (dynamic) pair.Value);
            var result = new Feature(geom, attributes);

            return result;
        }
    }
}
