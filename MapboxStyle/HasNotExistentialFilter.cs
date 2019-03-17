namespace MapboxStyle
{
    public class HasNotExistentialFilter : ExistentialFilter
    {
        public HasNotExistentialFilter(string key) : base(key)
        {
        }

        protected override bool OnEvaluate()
        {
            return false;
        }
    }
}