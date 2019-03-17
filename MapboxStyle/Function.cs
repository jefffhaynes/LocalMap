using System;
using System.Collections.Generic;
using System.Linq;

namespace MapboxStyle
{
    public abstract class Function<T>
    {
        private readonly IDictionary<double, T> _stops;
        private readonly T _constValue;
        private readonly bool _isConst;

        internal Function(IDictionary<double, T> stops)
        {
            _stops = stops;
        }

        internal Function(T constValue)
        {
            _constValue = constValue;
            _isConst = true;
        }

        public T GetValue(double zoom)
        {
            if (_isConst)
            {
                return _constValue;
            }

            var start = _stops.FirstOrDefault(pair => pair.Key < zoom);
            var end = _stops.FirstOrDefault(pair => pair.Key > zoom);

            if (start.Equals(default(KeyValuePair<double, T>)))
            {
                return end.Value;
            }
            
            if (end.Equals(default(KeyValuePair<double, T>)))
            {
                return start.Value;
            }

            return Interpolate(start.Key, end.Key, start.Value, end.Value, zoom);
        }

        protected abstract T Interpolate(double x0, double x1, T y0, T y1, double x);


        protected double InterpolateDouble(double x0, double x1, double y0, double y1, double x)
        {
            if (Math.Abs(x1 - x0) < double.Epsilon)
            {
                return (y0 + y1) / 2;
            }

            return y0 + (x - x0) * (y1 - y0) / (x1 - x0);
        }
    }
}