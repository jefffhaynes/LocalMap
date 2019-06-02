using System.Collections.Generic;

namespace MapboxStyle
{
    public class StringFunction : Function<string>
    {
        internal StringFunction(IDictionary<double, string> stops, double baseValue = 1.0) : base(stops, baseValue)
        {
        }

        internal StringFunction(string constValue) : base(constValue)
        {
        }

        protected override string Interpolate(double x0, double x1, string y0, string y1, double x)
        {
            return y0;
        }
    }
}