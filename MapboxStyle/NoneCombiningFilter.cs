using System.Collections.Generic;
using System.Linq;

namespace MapboxStyle
{
    public class NoneCombiningFilter : CombiningFilter
    {
        public NoneCombiningFilter(IEnumerable<Filter> filters) : base(filters)
        {
        }

        public override bool Evaluate(FilterType featureType, string featureId, double zoom, IDictionary<string, string> featureProperties)
        {
            return Filters.All(filter => !filter.Evaluate(featureType, featureId, zoom, featureProperties));
        }
    }
}