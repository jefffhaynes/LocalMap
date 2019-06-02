using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MapboxStyle
{
    class StringFunctionConverter : JsonConverter
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

                var stops = stopItems.ToDictionary(pair => (double) pair.First, pair => pair.Last.ToString());

                if (item.TryGetValue("base", StringComparison.CurrentCultureIgnoreCase, out var baseValue))
                {
                    return new StringFunction(stops, (double) baseValue);
                }

                return new StringFunction(stops);
            }

            if (value is string s)
            {
                return new StringFunction(s);
            }

            throw new NotSupportedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(StringFunction);
        }
    }
}
