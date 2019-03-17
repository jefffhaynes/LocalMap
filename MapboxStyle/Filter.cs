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
        protected KeyedFilter(string key)
        {
            Key = key;
        }

        public string Key { get; }

        public override bool Evaluate(FilterType featureType, string featureId, IDictionary<string, string> featureProperties)
        {
            var properties = featureProperties.ToDictionary(pair => pair.Key, pair => pair.Value);
            properties.Add("$type", featureType.ToString());
            properties.Add("$id", featureId);
            return OnEvaluate(properties);
        }

        protected abstract bool OnEvaluate(IDictionary<string, string> properties);
    }

    public abstract class ComparisonFilter : KeyedFilter
    {
        protected ComparisonFilter(string key, string value) : base(key)
        {
            Value = value;
        }

        public string Value { get; }

        protected override bool OnEvaluate(IDictionary<string, string> properties)
        {
            if (!properties.TryGetValue(Key, out string value))
            {
                return false;
            }

            return OnEvaluate(Value, value);
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
