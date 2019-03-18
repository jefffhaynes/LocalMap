namespace MapboxStyle
{
    public class LessThanOrEqualFilter : ComparisonFilter
    {
        public LessThanOrEqualFilter(string key, string value) : base(key, value)
        {
        }

        protected override bool OnEvaluate(string left, string right)
        {
            return double.Parse(left) <= double.Parse(right);
        }
    }
}