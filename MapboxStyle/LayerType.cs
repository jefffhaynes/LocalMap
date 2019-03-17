using Newtonsoft.Json;

namespace MapboxStyle
{
    public enum LayerType
    {
        [JsonProperty("background")]
        Background,

        [JsonProperty("fill")]
        Fill,

        [JsonProperty("line")]
        Line,

        [JsonProperty("symbol")]
        Symbol,

        [JsonProperty("raster")]
        Raster,

        [JsonProperty("circle")]
        Circle,

        [JsonProperty("fill-extrusion")]
        FillExtrusion,

        [JsonProperty("heatmap")]
        Heatmap,

        [JsonProperty("hillshade")]
        Hillshade
    }
}