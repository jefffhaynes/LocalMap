using Newtonsoft.Json;

namespace MapboxStyle
{
    public enum Visibility
    {
        [JsonProperty("visible")]
        Visible,

        [JsonProperty("none")]
        None
    }
}