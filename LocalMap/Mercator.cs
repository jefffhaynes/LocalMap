using System;
using System.Numerics;

namespace LocalMap
{
    public static class Mercator
    {
        private const double Radius = 6378137;
        private const double E = 0.0818191908426;
        private const double DegreesToRadians = Math.PI / 180;
        private const double HalfPi = Math.PI / 2;
        private const double PiDiv4 = Math.PI / 4;

        private const double C1 = 0.00335655146887969;
        private const double C2 = 0.00000657187271079536;
        private const double C3 = 0.00000001764564338702;
        private const double C4 = 0.00000000005328478445;

        public static Vector2 Convert(Geoposition position)
        {
            var lonRadians = DegreesToRadians * position.Longitude;
            var latRadians = DegreesToRadians * position.Latitude;

            var x = Radius * lonRadians;
            //y=a×ln[tan(π/4+φ/2)×((1-e×sinφ)/(1+e×sinφ))^(e/2)]
            var y = Radius * Math.Log(Math.Tan(PiDiv4 + latRadians * 0.5) / Math.Pow(Math.Tan(PiDiv4 + Math.Asin(E * Math.Sin(latRadians)) / 2), E));

            return new Vector2((float)x, (float)y);
        }

        public static Geoposition Convert(Vector2 value)
        {
            double g = HalfPi - 2 * Math.Atan(1 / Math.Exp(value.Y / Radius));
            double latRadians = g + C1 * Math.Sin(2 * g) + C2 * Math.Sin(4 * g) + C3 * Math.Sin(6 * g) + C4 * Math.Sin(8 * g);

            var lonRadians = value.X / Radius;

            var lon = lonRadians / DegreesToRadians;
            var lat = latRadians / DegreesToRadians;

            return new Geoposition((float)lon, (float)lat);
        }
    }
}
