using System.Collections.Generic;
using Newtonsoft.Json;

namespace MapboxStyle
{
    public class Layout
    {
        [JsonProperty("text-field")]
        public string TextField { get; set; }

        [JsonProperty("text-font")]
        public List<string> TextFont { get; set; }

        [JsonProperty("text-size")]
        public DoubleFunction TextSize { get; set; }

        [JsonProperty("text-anchor")]
        public TextAnchor? TextAnchor { get; set; }

        [JsonProperty("text-padding")]
        public DoubleFunction TextPadding { get; set; }

        [JsonProperty("text-transform")]
        public TextTransform? TextTransform { get; set; }

        [JsonProperty("line-cap")]
        public LineCap? LineCap { get; set; }

        [JsonProperty("line-join")]
        public LineJoin? LineJoin { get; set; }

        [JsonProperty("line-dasharray")]
        public List<float> LineDashArray { get; set; }

        [JsonProperty("symbol-placement")]
        public SymbolPlacement SymbolPlacement { get; set; }

        [JsonProperty("symbol-spacing")]
        public DoubleFunction SymbolSpacing { get; set; }
    }
}
