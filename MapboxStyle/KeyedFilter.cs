using System.Collections.Generic;

namespace MapboxStyle
{
    public abstract class KeyedFilter : Filter
    {
        private readonly string _key;

        protected KeyedFilter(string key)
        {
            _key = key;
        }

        public override bool Evaluate(FilterType featureType, string featureId,
            IDictionary<string, string> featureProperties)
        {
            if (_key == "$type")
            {
                return OnEvaluate(featureType.ToString());
            }

            if (_key == "$id")
            {
                return OnEvaluate(featureId);
            }

            return featureProperties.TryGetValue(_key, out string value) && OnEvaluate(value);
        }

        protected abstract bool OnEvaluate(string value);
    }
}