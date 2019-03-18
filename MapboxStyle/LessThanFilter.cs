namespace MapboxStyle
{
    public class LessThanFilter : ComparisonFilter
    {
        public LessThanFilter(string key, string value) : base(key, value)
        {
        }

        protected override bool OnEvaluate(string left, string right)
        {
            return double.Parse(left) < double.Parse(right);
        }
    }
}