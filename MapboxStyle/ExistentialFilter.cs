namespace MapboxStyle
{
    public abstract class ExistentialFilter : KeyedFilter
    {
        protected ExistentialFilter(string key) : base(key)
        {
        }

        protected override bool OnEvaluate(string value)
        {
            return OnEvaluate();
        }

        protected abstract bool OnEvaluate();
    }
}