using System.Collections.Generic;
using System.Linq;

namespace MapboxStyle
{
    public class AllCombiningFilter : CombiningFilter
    {
        public AllCombiningFilter(IEnumerable<Filter> filters) : base(filters)
        {
        }

        public override object Evaluate(FilterType featureType, string featureId, double zoom, IDictionary<string, string> featureProperties)
        {
            return Filters.All(filter => (bool) filter.Evaluate(featureType, featureId, zoom, featureProperties));
        }
    }
}