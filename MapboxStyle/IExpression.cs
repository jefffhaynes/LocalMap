using System.Collections.Generic;

namespace MapboxStyle
{
    public interface IExpression<T>
    {
        T Evaluate(FilterType featureType, string featureId, double zoom, IDictionary<string, string> featureProperties);
    }
}
