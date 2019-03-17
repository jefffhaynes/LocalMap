﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MapboxStyle
{
    class DoubleFunctionConverter : JsonConverter
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

                var stops = stopItems.ToDictionary(pair => (double) pair.First, pair => (double) pair.Last);

                return new DoubleFunction(stops);
            }

            if (value is double d)
            {
                return new DoubleFunction(d);
            }
            
            if (value is int i)
            {
                return new DoubleFunction(i);
            }

            if (value is long l)
            {
                return new DoubleFunction(l);
            }

            throw new NotSupportedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DoubleFunction);
        }
    }
}
