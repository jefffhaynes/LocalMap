using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace MapboxStyle
{
    public enum LayerType
    {
        [EnumMember(Value="background")]
        Background,

        [EnumMember(Value="fill")]
        Fill,

        [EnumMember(Value="line")]
        Line,

        [EnumMember(Value ="symbol")]
        Symbol,

        [EnumMember(Value = "raster")]
        Raster,

        [EnumMember(Value = "circle")]
        Circle,

        [EnumMember(Value = "fill-extrusion")]
        FillExtrusion,

        [EnumMember(Value = "heatmap")]
        Heatmap,

        [EnumMember(Value = "hillshade")]
        Hillshade
    }
}