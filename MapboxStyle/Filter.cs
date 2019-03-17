using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MapboxStyle
{
    public abstract class Filter
    {
        public abstract bool Evaluate(FilterType featureType, string featureId, IDictionary<string, string> featureProperties);
    }

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

    public class InMembershipFilter : MembershipFilter
    {
        public InMembershipFilter(string key, IEnumerable<string> values) : base(key, values)
        {
        }

        protected override bool OnEvaluate(string left, IEnumerable<string> right)
        {
            return right.Contains(left);
        }
    }

    public class NotInMembershipFilter : MembershipFilter
    {
        public NotInMembershipFilter(string key, IEnumerable<string> values) : base(key, values)
        {
        }

        protected override bool OnEvaluate(string left, IEnumerable<string> right)
        {
            return !right.Contains(left);
        }
    }

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

    public class EqualFilter : ComparisonFilter
    {
        public EqualFilter(string key, string value) : base(key, value)
        {
        }

        protected override bool OnEvaluate(string left, string right)
        {
            return left.Equals(right, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    public class NotEqualFilter : ComparisonFilter
    {
        public NotEqualFilter(string key, string value) : base(key, value)
        {
        }

        protected override bool OnEvaluate(string left, string right)
        {
            return !left.Equals(right, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    public abstract class CombiningFilter : Filter
    {
        protected CombiningFilter(IEnumerable<Filter> filters)
        {
            Filters = filters;
        }

        public IEnumerable<Filter> Filters { get; }
    }

    public class AllCombiningFilter : CombiningFilter
    {
        public AllCombiningFilter(IEnumerable<Filter> filters) : base(filters)
        {
        }

        public override bool Evaluate(FilterType featureType, string featureId, IDictionary<string, string> featureProperties)
        {
            return Filters.All(filter => filter.Evaluate(featureType, featureId, featureProperties));
        }
    }

    public class AnyCombiningFilter : CombiningFilter
    {
        public AnyCombiningFilter(IEnumerable<Filter> filters) : base(filters)
        {
        }

        public override bool Evaluate(FilterType featureType, string featureId, IDictionary<string, string> featureProperties)
        {
            return Filters.Any(filter => filter.Evaluate(featureType, featureId, featureProperties));
        }
    }

    public class NoneCombiningFilter : CombiningFilter
    {
        public NoneCombiningFilter(IEnumerable<Filter> filters) : base(filters)
        {
        }

        public override bool Evaluate(FilterType featureType, string featureId, IDictionary<string, string> featureProperties)
        {
            return Filters.All(filter => !filter.Evaluate(featureType, featureId, featureProperties));
        }
    }

    public enum FilterType
    {
        Point,
        LineString,
        Polygon
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum FilterOperator
    {
        [EnumMember(Value = "has")]
        Exists,

        [EnumMember(Value = "!has")]
        DoesNotExist,

        [EnumMember(Value = "==")]
        Equal,

        [EnumMember(Value = "!=")]
        NotEqual,

        [EnumMember(Value = ">")]
        GreaterThan,

        [EnumMember(Value = ">=")]
        GreaterThanOrEqual,

        [EnumMember(Value = "<")]
        LessThan,

        [EnumMember(Value = "<=")]
        LessThanOrEqual,

        [EnumMember(Value = "in")]
        Inclusion,

        [EnumMember(Value = "!in")]
        Exclusion,

        [EnumMember(Value = "all")]
        All,

        [EnumMember(Value = "any")]
        Any,

        [EnumMember(Value = "none")]
        None
    }
}
