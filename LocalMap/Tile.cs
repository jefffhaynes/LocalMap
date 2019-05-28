namespace MapTest
{
    public class Tile
    {
        public Tile(int x, int y, int zoomLevel)
        {
            X = x;
            Y = y;
            ZoomLevel = zoomLevel;
        }

        public int X { get; }

        public int Y { get; }

        public int ZoomLevel { get; }
    }
}
