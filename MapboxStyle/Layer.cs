using Newtonsoft.Json;

namespace MapboxStyle
{
    public class Layer
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("source-layer")]
        public string SourceLayer { get; set; }

        [JsonProperty("type")]
        public LayerType Type { get; set; }

        [JsonProperty("paint")]
        public Paint Paint { get; set; }
    }
}