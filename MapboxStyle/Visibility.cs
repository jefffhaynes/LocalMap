using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MapboxStyle
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Visibility
    {
        [EnumMember(Value = "visible")]
        Visible,

        [EnumMember(Value = "none")]
        None
    }
}