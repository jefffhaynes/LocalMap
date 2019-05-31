using System.Collections.Generic;

namespace MapboxStyle
{
    public class BoolFunction : Function<bool>
    {
        public BoolFunction(IDictionary<double, bool> stops, double baseValue) : base(stops, baseValue)
        {
        }

        public BoolFunction(bool constValue) : base(constValue)
        {
        }

        protected override bool Interpolate(double x0, double x1, bool y0, bool y1, double x)
        {
            throw new System.NotImplementedException();
        }
    }
}