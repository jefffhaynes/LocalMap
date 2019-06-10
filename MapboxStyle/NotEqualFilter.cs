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
            if (left == null && right == null)
            {
                return false;
            }

            if (left == null || right == null)
            {
                return true;
            }

            return !left.Equals(right, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}