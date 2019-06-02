using System;
using System.Collections.Generic;
using System.Drawing;

namespace MapboxStyle
{
    public abstract class Expression
    {
        public double GetDouble(FilterType featureType, string featureId, double zoom,
            IDictionary<string, string> featureProperties)
        {
            return (double) Evaluate(featureType, featureId, zoom, featureProperties);
        }

        public Color GetColor(FilterType featureType, string featureId, double zoom,
            IDictionary<string, string> featureProperties)
        {
            return (Color) Evaluate(featureType, featureId, zoom, featureProperties);
        }

        public bool GetBool(FilterType featureType, string featureId, double zoom,
            IDictionary<string, string> featureProperties)
        {
            var value = Evaluate(featureType, featureId, zoom, featureProperties);
            return (bool?) value ?? false;
        }

        public Array GetArray(FilterType featureType, string featureId, double zoom,
            IDictionary<string, string> featureProperties)
        {
            return (Array)Evaluate(featureType, featureId, zoom, featureProperties);
        }

        public string GetString(FilterType featureType, string featureId, double zoom,
            IDictionary<string, string> featureProperties)
        {
            return (string)Evaluate(featureType, featureId, zoom, featureProperties);
        }

        public abstract object Evaluate(FilterType featureType, string featureId, double zoom, IDictionary<string, string> featureProperties);
    }

    //public abstract class TypedExpression<T> : IExpression
    //{
    //    public abstract object Evaluate(FilterType featureType, string featureId, double zoom,
    //        IDictionary<string, string> featureProperties);

    //    public T GetValue(FilterType featureType, string featureId, double zoom,
    //        IDictionary<string, string> featureProperties)
    //    {
    //        return (T) Evaluate(featureType, featureId, zoom, featureProperties);
    //    }
    //}
}
