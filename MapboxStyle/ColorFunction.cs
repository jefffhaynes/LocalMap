using System.Collections.Generic;
using System.Drawing;

namespace MapboxStyle
{
    public class ColorFunction : Function<Color>
    {
        internal ColorFunction(IDictionary<double, Color> stops, double baseValue = 1.0) : base(stops, baseValue)
        {
        }

        internal ColorFunction(Color constValue) : base(constValue)
        {
        }

        protected override Color Interpolate(double x0, double x1, Color y0, Color y1, double x)
        {
            var hslaColor0 = HslaColor.FromColor(y0);
            var hslaColor1 = HslaColor.FromColor(y1);

            var h = InterpolateDouble(x0, x1, hslaColor0.H, hslaColor1.H, x);
            var s = InterpolateDouble(x0, x1, hslaColor0.S, hslaColor1.S, x);
            var l = InterpolateDouble(x0, x1, hslaColor0.L, hslaColor1.L, x);
            var a = InterpolateDouble(x0, x1, y0.A, y1.A, x);

            var hslaColor = new HslaColor(h, s, l, a);

            return hslaColor.ToColor();
        }
    }
}