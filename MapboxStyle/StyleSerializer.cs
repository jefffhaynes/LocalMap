using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MapboxStyle
{
    public static class StyleSerializer
    {
        public static Style Deserialize(Stream stream)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ColorFunctionConverter());
            settings.Converters.Add(new DoubleFunctionConverter());

            var reader = new StreamReader(stream);
            var jsonReader = new JsonTextReader(reader);

            var serializer = JsonSerializer.Create(settings);
            return serializer.Deserialize<Style>(jsonReader);
        }
    }
}
