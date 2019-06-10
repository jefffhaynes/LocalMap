using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MapboxStyle
{
    class BoolFunctionConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;

            if (value is null)
            {
                var item = JObject.Load(reader);
                var stopItems = item["stops"];

                var stops = stopItems.ToDictionary(pair => (double)pair.First, pair => (bool) pair.Last);

                if (item.TryGetValue("base", StringComparison.CurrentCultureIgnoreCase, out var baseValue))
                {
                    return new BoolFunction(stops, (double) baseValue);
                }

                return new BoolFunction(stops);
            }

            if (value is bool b)
            {
                return new BoolFunction(b);
            }

            throw new NotSupportedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(BoolFunction);
        }
    }
}