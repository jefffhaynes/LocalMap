using System.Collections.Generic;

namespace MapboxStyle
{
    public abstract class Filter : IExpression<bool>
    {
        public abstract bool Evaluate(FilterType featureType, string featureId, double zoom, IDictionary<string, string> featureProperties);
    }
}
