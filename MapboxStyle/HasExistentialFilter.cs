namespace MapboxStyle
{
    public class HasExistentialFilter : ExistentialFilter
    {
        public HasExistentialFilter(string key) : base(key)
        {
        }

        protected override bool OnEvaluate()
        {
            return true;
        }
    }
}