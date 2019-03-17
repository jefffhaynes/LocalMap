using Newtonsoft.Json;

namespace MapboxStyle
{
    public enum LineCap
    {
        [JsonProperty("butt")]
        Butt,

        [JsonProperty("round")]
        Round,

        [JsonProperty("square")]
        Square
    }
}