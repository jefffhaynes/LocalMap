using System.Collections.Generic;
using System.Linq;

namespace MapboxStyle
{
    public class AnyCombiningFilter : CombiningFilter
    {
        public AnyCombiningFilter(IEnumerable<Filter> filters) : base(filters)
        {
        }

        public override bool Evaluate(FilterType featureType, string featureId, IDictionary<string, string> featureProperties)
        {
            return Filters.Any(filter => filter.Evaluate(featureType, featureId, featureProperties));
        }
    }
}