using System.Collections.Generic;

namespace MapboxStyle
{
    public abstract class Filter
    {
        public abstract bool Evaluate(FilterType featureType, string featureId, IDictionary<string, string> featureProperties);
    }
}
