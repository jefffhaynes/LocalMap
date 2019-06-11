using System;
using System.Drawing;

namespace MapboxStyle
{
    public class HslaColor
    {
        public HslaColor(double h, double s, double l, double a = 1.0)
        {
            CheckRange(h, nameof(h));
            CheckRange(s, nameof(s));
            CheckRange(l, nameof(l));
            CheckRange(a, nameof(a));

            H = h;
            S = s;
            L = l;
            A = a;
        }

        public double H { get; }

        public double S { get; }

        public double L { get; }

        public double A { get; }

        public Color ToColor()
        {
            return ColorFromHsl(H, S, L, A);
        }

        public static HslaColor FromColor(Color color)
        {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;

            float min = Math.Min(Math.Min(r, g), b);
            float max = Math.Max(Math.Max(r, g), b);
            float delta = max - min;

            float h = 0;
            float s = 0;
            float l = (max + min) / 2.0f;

            if (Math.Abs(delta) > float.Epsilon)
            {
                if (l < 0.5f)
                {
                    s = delta / (max + min);
                }
                else
                {
                    s = delta / (2.0f - max - min);
                }


                if (Math.Abs(r - max) < float.Epsilon)
                {
                    h = (g - b) / delta;
                }
                else if (Math.Abs(g - max) < float.Epsilon)
                {
                    h = 2f + (b - r) / delta;
                }
                else if (Math.Abs(b - max) < float.Epsilon)
                {
                    h = 4f + (r - g) / delta;
                }
            }

            return new HslaColor(h, s, l);
        }

        private static Color ColorFromHsl(double h, double s, double l, double a = 1.0)
        {
            byte r;
            byte g;
            byte b;

            if (Math.Abs(s) < double.Epsilon)
            {
                r = g = b = (byte)(l * 255);
            }
            else
            {
                float hue = (float)h;

                var v2 = (float) (l < 0.5 ? l * (1 + s) : l + s - l * s);
                var v1 = (float) (2 * l - v2);

                r = (byte)(255 * HueToRgb(v1, v2, hue + 1.0f / 3));
                g = (byte)(255 * HueToRgb(v1, v2, hue));
                b = (byte)(255 * HueToRgb(v1, v2, hue - 1.0f / 3));
            }

            return Color.FromArgb((int)(a * byte.MaxValue), r, g, b);
        }

        private static double HueToRgb(double v1, double v2, double vH)
        {
            if (vH < 0.0)
            {
                vH += 1.0;
            }
            else if (vH > 1.0)
            {
                vH -= 1.0;
            }

            if (vH < 1.0 / 6.0)
            {
                return v1 + (v2 - v1) * 6.0 * vH;
            }

            if (vH < 0.5)
            {
                return v2;
            }

            if (vH < 2.0 / 3.0)
            {
                return v1 + (v2 - v1) * (2.0 / 3.0 - vH) * 6.0;
            }

            return v1;
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private void CheckRange(double value, string parameterName)
        {
            if (value < 0 || value > 1.0)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}
