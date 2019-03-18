namespace MapboxStyle
{
    public class GreaterThanOrEqualFilter : ComparisonFilter
    {
        public GreaterThanOrEqualFilter(string key, string value) : base(key, value)
        {
        }

        protected override bool OnEvaluate(string left, string right)
        {
            return double.Parse(left) >= double.Parse(right);
        }
    }
}