using System;

namespace MapboxStyle
{
    public class NotEqualFilter : ComparisonFilter
    {
        public NotEqualFilter(string key, string value) : base(key, value)
        {
        }

        protected override bool OnEvaluate(string left, string right)
        {
            return !left.Equals(right, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}