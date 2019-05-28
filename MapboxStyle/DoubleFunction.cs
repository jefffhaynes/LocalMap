using System.Collections.Generic;

namespace MapboxStyle
{
    public class DoubleFunction : Function<double>
    {
        internal DoubleFunction(IDictionary<double, double> stops, double baseValue = 1.0) : base(stops, baseValue)
        {
        }

        internal DoubleFunction(double constValue) : base(constValue)
        {
        }

        protected override double Interpolate(double x0, double x1, double y0, double y1, double x)
        {
            return InterpolateDouble(x0, x1, y0, y1, x);
        }
    }
}