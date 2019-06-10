using System.Collections.Generic;

namespace MapboxStyle
{
    public class BoolFunction : Function<bool>
    {
        internal BoolFunction(IDictionary<double, bool> stops, double baseValue = 1.0) : base(stops, baseValue)
        {
        }

        internal BoolFunction(bool constValue) : base(constValue)
        {
        }

        protected override bool Interpolate(double x0, double x1, bool y0, bool y1, double x)
        {
            return y0;
        }
    }
}