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
            if (left == null && right == null)
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            return left.Equals(right, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}