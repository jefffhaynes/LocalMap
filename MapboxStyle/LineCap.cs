using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MapboxStyle
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LineCap
    {
        [EnumMember(Value = "butt")]
        Butt,

        [EnumMember(Value = "round")]
        Round,

        [EnumMember(Value = "square")]
        Square
    }
}