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
                    break;
                case FilterOperator.DoesNotExist:
                    break;
                case FilterOperator.Equal:
                    return new EqualFilter(array[1].ToString(), array[2].ToString());
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
                case FilterOperator.Inclusion:
                    break;
                case FilterOperator.Exclusion:
                    break;
                case FilterOperator.All:
                    var arrays = array.Skip(1).Select(token => (JArray) token);
                    var filters = arrays.Select(GetFilter).ToList();
                    return new AllCombiningFilter(filters);
                case FilterOperator.Any:
                    break;
                case FilterOperator.None:
                    break;
            }

            return null;
        }
    }
}
