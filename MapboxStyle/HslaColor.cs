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
            float r = (color.R / 255f);
            float g = (color.G / 255f);
            float b = (color.B / 255f);

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
            double r = 0, g = 0, b = 0;
            if (Math.Abs(l) > double.Epsilon)
            {
                if (Math.Abs(s) < double.Epsilon)
                {
                    r = g = b = l;
                }
                else
                {
                    double temp2;
                    if (l < 0.5)
                    {
                        temp2 = l * (1.0 + s);
                    }
                    else
                    {
                        temp2 = l + s - l * s;
                    }

                    var temp1 = 2.0 * l - temp2;

                    r = GetColorComponent(temp1, temp2, h + 1.0 / 3.0);
                    g = GetColorComponent(temp1, temp2, h);
                    b = GetColorComponent(temp1, temp2, h - 1.0 / 3.0);
                }
            }

            return Color.FromArgb(
                (int)(a * byte.MaxValue),
                (int)(byte.MaxValue * r),
                (int)(byte.MaxValue * g),
                blue: (int)(byte.MaxValue * b));
        }

        private static double GetColorComponent(double temp1, double temp2, double temp3)
        {
            if (temp3 < 0.0)
            {
                temp3 += 1.0;
            }
            else if (temp3 > 1.0)
            {
                temp3 -= 1.0;
            }

            if (temp3 < 1.0 / 6.0)
            {
                return temp1 + (temp2 - temp1) * 6.0 * temp3;
            }

            if (temp3 < 0.5)
            {
                return temp2;
            }

            if (temp3 < 2.0 / 3.0)
            {
                return temp1 + (temp2 - temp1) * (2.0 / 3.0 - temp3) * 6.0;
            }

            return temp1;
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
