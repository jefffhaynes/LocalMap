using System.Collections.Generic;

namespace MapboxStyle
{
    public abstract class CombiningFilter : Filter
    {
        protected CombiningFilter(IEnumerable<Filter> filters)
        {
            Filters = filters;
        }

        public IEnumerable<Filter> Filters { get; }
    }
}