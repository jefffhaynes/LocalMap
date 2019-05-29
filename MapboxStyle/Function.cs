using System;
using System.Collections.Generic;
using System.Linq;

namespace MapboxStyle
{
    public abstract class Function<T> : IExpression<T>
    {
        private readonly IDictionary<double, T> _stops;
        private readonly double _base;
        private readonly T _constValue;
        private readonly bool _isConst;

        internal Function(IDictionary<double, T> stops, double baseValue)
        {
            _stops = stops;
            _base = baseValue;
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
                return _stops.First().Value;
            }
            
            if (end.Equals(default(KeyValuePair<double, T>)))
            {
                return _stops.Last().Value;
            }

            return Interpolate(start.Key, end.Key, start.Value, end.Value, zoom);
        }

        protected abstract T Interpolate(double x0, double x1, T y0, T y1, double x);


        protected double InterpolateDouble(double x0, double x1, double y0, double y1, double x)
        {
            var factor = GetInterpolationFactor(x0, x1, x);
            return (y1 - y0) * factor + y0;
        }

        private double GetInterpolationFactor(double x0, double x1, double x)
        {
            var delta = x1 - x0;
            var progress = x - x0;
            if (Math.Abs(delta) < double.Epsilon)
            {
                return 0;
            }

            if (Math.Abs(_base - 1.0f) < double.Epsilon)
            {
                return progress / delta;
            }

            return (Math.Pow(progress, _base) - 1) /
                   (Math.Pow(delta, _base) - 1);
        }
    }
}