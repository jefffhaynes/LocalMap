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
                OnEvaluate(featureType.ToString());
            }
            else if (_key == "$id")
            {
                OnEvaluate(featureId);
            }
            else if (featureProperties.TryGetValue(_key, out string value))
            {
                OnEvaluate(value);
            }

            return false;
        }

        protected abstract bool OnEvaluate(string value);
    }
}