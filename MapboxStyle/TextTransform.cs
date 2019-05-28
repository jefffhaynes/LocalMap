using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MapboxStyle
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TextTransform
    {
        [EnumMember(Value = "none")]
        None,
        [EnumMember(Value = "uppercase")]
        Uppercase,
        [EnumMember(Value = "lowercase")]
        Lowercase
    }
}
