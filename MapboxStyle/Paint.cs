using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;

namespace MapboxStyle
{
    public class Paint
    {
        [JsonProperty("visibility")]
        public Visibility Visibility { get; set; }

        [JsonProperty("background-color")]
        public Expression BackgroundColor { get; set; }

        [JsonProperty("background-pattern")]
        public string BackgroundPattern { get; set; }

        [JsonProperty("background-opacity")]
        public Expression BackgroundOpacity { get; set; }

        [JsonProperty("fill-antialias")]
        public bool? FillAntialias { get; set; }

        [JsonProperty("fill-opacity")]
        public Expression FillOpacity { get; set; }

        [JsonProperty("fill-color")]
        public Expression FillColor { get; set; }

        [JsonProperty("fill-outline-color")]
        public Expression FillOutlineColor { get; set; }

        [JsonProperty("fill-translate")]
        public List<double> FillTranslate { get; set; }

        [JsonProperty("fill-pattern")]
        public string FillPattern { get; set; }

        [JsonProperty("line-miter-limit")]
        public Expression LineMiterLimit { get; set; }

        [JsonProperty("line-round-limit")]
        public Expression LineRoundLimit { get; set; }

        [JsonProperty("line-opacity")]
        public Expression LineOpacity { get; set; }

        [JsonProperty("line-color")]
        public Expression LineColor { get; set; }

        [JsonProperty("line-translate")]
        public List<double> LineTranslate { get; set; }

        [JsonProperty("line-width")]
        public Expression LineWidth { get; set; }

        [JsonProperty("line-gap-width")]
        public Expression LineGapWidth { get; set; }

        [JsonProperty("line-offset")]
        public Expression LineOffset { get; set; }

        [JsonProperty("line-blur")]
        public Expression LineBlur { get; set; }

        [JsonProperty("line-dasharray")]
        public Expression LineDashArray { get; set; }

        [JsonProperty("line-pattern")]
        public string LinePattern { get; set; }

        [JsonProperty("text-color")]
        public Expression TextColor { get; set; }

        [JsonProperty("text-halo-blur")]
        public Expression TextHaloBlur { get; set; }

        [JsonProperty("text-halo-color")]
        public Expression TextHaloColor { get; set; }

        [JsonProperty("text-halo-width")]
        public Expression TextHaloWidth { get; set; }
    }
}