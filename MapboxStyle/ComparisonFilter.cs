namespace MapboxStyle
{
    public abstract class ComparisonFilter : KeyedFilter
    {
        private readonly string _value;

        protected ComparisonFilter(string key, string value) : base(key)
        {
            _value = value;
        }

        protected override bool OnEvaluate(string value)
        {
            return OnEvaluate(value, _value);
        }

        protected abstract bool OnEvaluate(string left, string right);
    }
}