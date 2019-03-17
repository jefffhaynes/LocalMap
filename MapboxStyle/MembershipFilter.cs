using System.Collections.Generic;

namespace MapboxStyle
{
    public abstract class MembershipFilter : KeyedFilter
    {
        private readonly IEnumerable<string> _values;

        protected MembershipFilter(string key, IEnumerable<string> values) : base(key)
        {
            _values = values;
        }
        
        protected override bool OnEvaluate(string value)
        {
            return OnEvaluate(value, _values);
        }

        protected abstract bool OnEvaluate(string left, IEnumerable<string> right);
    }
}