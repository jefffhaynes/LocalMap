using System.Collections.Generic;
using System.Drawing;

namespace MapboxStyle
{
    public class ColorFunction : Function<Color>
    {
        internal ColorFunction(IDictionary<double, Color> stops) : base(stops)
        {
        }

        internal ColorFunction(Color constValue) : base(constValue)
        {
        }

        protected override Color Interpolate(double x0, double x1, Color y0, Color y1, double x)
        {
            ColorUtils.RgbToHls(y0.R, y0.G, y0.B, out var h0, out var l0, out var s0);
            ColorUtils.RgbToHls(y1.R, y1.G, y1.B, out var h1, out var l1, out var s1);

            var h2 = InterpolateDouble(x0, x1, h0, h1, x);
            var l2 = InterpolateDouble(x0, x1, l0, l1, x);
            var s2 = InterpolateDouble(x0, x1, s0, s1, x);
            var a2 = InterpolateDouble(x0, x1, y0.A, y1.A, x);

            ColorUtils.HlsToRgb(h2, l2, s2, out var r, out var g, out var b);

            return Color.FromArgb((byte) a2, r, g, b);
        }
    }
}