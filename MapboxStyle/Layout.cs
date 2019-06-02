using System.Collections.Generic;
using Newtonsoft.Json;

namespace MapboxStyle
{
    public class Layout
    {
        [JsonProperty("text-field")]
        public Expression TextField { get; set; }

        [JsonProperty("text-font")]
        public Expression TextFont { get; set; }

        [JsonProperty("text-size")]
        public Expression TextSize { get; set; }

        [JsonProperty("text-anchor")]
        public TextAnchor? TextAnchor { get; set; }

        [JsonProperty("text-padding")]
        public Expression TextPadding { get; set; }

        [JsonProperty("text-transform")]
        public TextTransform? TextTransform { get; set; }

        [JsonProperty("max-text-width")]
        public Expression MaximumTextWidth { get; set; }

        [JsonProperty("line-cap")]
        public LineCap? LineCap { get; set; }

        [JsonProperty("line-join")]
        public LineJoin? LineJoin { get; set; }

        [JsonProperty("symbol-placement")]
        public SymbolPlacement SymbolPlacement { get; set; }

        [JsonProperty("symbol-spacing")]
        public Expression SymbolSpacing { get; set; }
    }
}
