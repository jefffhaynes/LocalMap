using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MapboxStyle
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SymbolPlacement
    {
        [EnumMember(Value = "point")]
        Point,
        [EnumMember(Value = "line")]
        Line,
        [EnumMember(Value = "line-center")]
        LineCenter
    }
}
