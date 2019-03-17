using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MapboxStyle
{
    class ColorFunctionConverter : JsonConverter
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

                var stops = stopItems.ToDictionary(pair => (double) pair.First,
                    pair => ((string) pair.Last).ParseHtmlColor());

                return new ColorFunction(stops);
            }

            if (value is string s)
            {
                return new ColorFunction(s.ParseHtmlColor());
            }
            
            throw new NotSupportedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ColorFunction);
        }
    }
}
