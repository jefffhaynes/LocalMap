using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MapboxStyle
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LineJoin
    {
        [EnumMember(Value = "bevel")]
        Bevel,

        [EnumMember(Value = "round")]
        Round,

        [EnumMember(Value = "miter")]
        Miter
    }
}