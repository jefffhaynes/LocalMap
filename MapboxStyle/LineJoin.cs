using Newtonsoft.Json;

namespace MapboxStyle
{
    public enum LineJoin
    {
        [JsonProperty("bevel")]
        Bevel,

        [JsonProperty("round")]
        Round,

        [JsonProperty("miter")]
        Miter
    }
}