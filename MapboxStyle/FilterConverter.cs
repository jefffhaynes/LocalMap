using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MapboxStyle
{
    class FilterConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var array = JArray.Load(reader);
            return GetFilter(array);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Filter);
        }

        private static Filter GetFilter(JArray array)
        {
            var filterOperator = array.First.ToObject<FilterOperator>();
            
            switch (filterOperator)
            {
                case FilterOperator.Exists:
                case FilterOperator.DoesNotExist:
                    return GetExistentialFilter(filterOperator, array);
                case FilterOperator.Equal:
                case FilterOperator.NotEqual:
                case FilterOperator.GreaterThan:
                case FilterOperator.GreaterThanOrEqual:
                case FilterOperator.LessThan:
                case FilterOperator.LessThanOrEqual:
                    return GetComparisonFilter(filterOperator, array);
                case FilterOperator.Inclusion:
                case FilterOperator.Exclusion:
                    return GetMembershipFilter(filterOperator, array);
                case FilterOperator.All:
                case FilterOperator.Any:
                case FilterOperator.None:
                    return GetCombiningFilter(filterOperator, array);
            }

            return null;
        }

        private static CombiningFilter GetCombiningFilter(FilterOperator filterOperator, JArray array)
        {
            var arrays = array.Skip(1).Select(token => (JArray) token);
            var filters = arrays.Select(GetFilter).ToList();

            switch (filterOperator)
            {
                case FilterOperator.All:
                    return new AllCombiningFilter(filters);
                case FilterOperator.Any:
                    return new AnyCombiningFilter(filters);
                case FilterOperator.None:
                    return new NoneCombiningFilter(filters);
            }

            return null;
        }

        private static ComparisonFilter GetComparisonFilter(FilterOperator filterOperator, JArray array)
        {
            var key = array[1].ToString();
            var value = array[2].ToString();

            switch (filterOperator)
            {
                case FilterOperator.Exists:
                    break;
                case FilterOperator.DoesNotExist:
                    break;
                case FilterOperator.Equal:
                    return new EqualFilter(key, value);
                case FilterOperator.NotEqual:
                    break;
                case FilterOperator.GreaterThan:
                    break;
                case FilterOperator.GreaterThanOrEqual:
                    break;
                case FilterOperator.LessThan:
                    break;
                case FilterOperator.LessThanOrEqual:
                    break;
            }

            return null;
        }

        private static MembershipFilter GetMembershipFilter(FilterOperator filterOperator, JArray array)
        {
            var key = array[1].ToString();
            var values = array.Skip(2).Select(token => token.ToString());

            if (filterOperator == FilterOperator.Inclusion)
            {
                return new InMembershipFilter(key, values);
            }

            if(filterOperator == FilterOperator.Exclusion)
            {
                return new NotInMembershipFilter(key, values);
            }

            return null;
        }

        private static ExistentialFilter GetExistentialFilter(FilterOperator filterOperator, JArray array)
        {
            var key = array[1].ToString();

            if (filterOperator == FilterOperator.Exists)
            {
                return new HasExistentialFilter(key);
            }

            if (filterOperator == FilterOperator.DoesNotExist)
            {
                return new HasNotExistentialFilter(key);
            }

            return null;
        }
    }
}
