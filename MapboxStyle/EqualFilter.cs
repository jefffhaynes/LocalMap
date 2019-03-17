using System;

namespace MapboxStyle
{
    public class EqualFilter : ComparisonFilter
    {
        public EqualFilter(string key, string value) : base(key, value)
        {
        }

        protected override bool OnEvaluate(string left, string right)
        {
            return left.Equals(right, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}