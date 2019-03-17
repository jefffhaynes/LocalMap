using System;

namespace MapboxStyle
{
    public static class ColorUtils
    {
        public static void HlsToRgb(double h, double l, double s,
            out int r, out int g, out int b)
        {
            double p2;
            if (l <= 0.5) p2 = l * (1 + s);
            else p2 = l + s - l * s;

            double p1 = 2 * l - p2;
            double doubleR, doubleG, doubleB;

            if (Math.Abs(s) < double.Epsilon)
            {
                doubleR = l;
                doubleG = l;
                doubleB = l;
            }
            else
            {
                doubleR = QqhToRgb(p1, p2, h + 120);
                doubleG = QqhToRgb(p1, p2, h);
                doubleB = QqhToRgb(p1, p2, h - 120);
            }

            // Convert RGB to the 0 to 255 range.
            r = (int)(doubleR * 255.0);
            g = (int)(doubleG * 255.0);
            b = (int)(doubleB * 255.0);
        }

        public static void RgbToHls(int r, int g, int b,
            out double h, out double l, out double s)
        {
            // Convert RGB to a 0.0 to 1.0 range.
            double doubleR = r / 255.0;
            double doubleG = g / 255.0;
            double doubleB = b / 255.0;

            // Get the maximum and minimum RGB components.
            double max = doubleR;
            if (max < doubleG) max = doubleG;
            if (max < doubleB) max = doubleB;

            double min = doubleR;
            if (min > doubleG) min = doubleG;
            if (min > doubleB) min = doubleB;

            double diff = max - min;
            l = (max + min) / 2;
            if (Math.Abs(diff) < 0.00001)
            {
                s = 0;
                h = 0;  // H is really undefined.
            }
            else
            {
                if (l <= 0.5) s = diff / (max + min);
                else s = diff / (2 - max - min);

                double rDist = (max - doubleR) / diff;
                double gDist = (max - doubleG) / diff;
                double bDist = (max - doubleB) / diff;

                if (Math.Abs(doubleR - max) < double.Epsilon) h = bDist - gDist;
                else if (Math.Abs(doubleG - max) < double.Epsilon) h = 2 + rDist - bDist;
                else h = 4 + gDist - rDist;

                h = h * 60;
                if (h < 0) h += 360;
            }
        }

        private static double QqhToRgb(double q1, double q2, double hue)
        {
            if (hue > 360) hue -= 360;
            else if (hue < 0) hue += 360;

            if (hue < 60) return q1 + (q2 - q1) * hue / 60;
            if (hue < 180) return q2;
            if (hue < 240) return q1 + (q2 - q1) * (240 - hue) / 60;
            return q1;
        }
    }
}
