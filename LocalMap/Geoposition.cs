namespace LocalMap
{
    public class Geoposition
    {
        public Geoposition(double longitude, double latitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude { get; }

        public double Longitude { get; }

        public override string ToString()
        {
            return $"{Latitude:F}°, {Longitude:F}°";
        }
    }
}
