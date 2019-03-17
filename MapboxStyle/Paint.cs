using System.Collections.Generic;
using Newtonsoft.Json;

namespace MapboxStyle
{
    public class Paint
    {
        [JsonProperty("visibility")]
        public Visibility Visibility { get; set; }

        [JsonProperty("background-color")]
        public ColorFunction BackgroundColor { get; set; }

        [JsonProperty("background-pattern")]
        public string BackgroundPattern { get; set; }

        [JsonProperty("background-opacity")]
        public DoubleFunction BackgroundOpacity { get; set; }

        [JsonProperty("fill-antialias")]
        public bool? FillAntialias { get; set; }

        [JsonProperty("fill-opacity")]
        public DoubleFunction FillOpacity { get; set; }

        [JsonProperty("fill-color")]
        public ColorFunction FillColor { get; set; }

        [JsonProperty("fill-outline-color")]
        public ColorFunction FillOutlineColor { get; set; }

        [JsonProperty("fill-translate")]
        public List<double> FillTranslate { get; set; }

        [JsonProperty("fill-pattern")]
        public string FillPattern { get; set; }

        [JsonProperty("line-cap")]
        public LineCap? LineCap { get; set; }

        [JsonProperty("line-join")]
        public LineJoin? LineJoin { get; set; }

        [JsonProperty("line-miter-limit")]
        public DoubleFunction LineMiterLimit { get; set; }

        [JsonProperty("line-round-limit")]
        public DoubleFunction LineRoundLimit { get; set; }

        [JsonProperty("line-opacity")]
        public DoubleFunction LineOpacity { get; set; }

        [JsonProperty("line-color")]
        public ColorFunction LineColor { get; set; }

        [JsonProperty("line-translate")]
        public List<double> LineTranslate { get; set; }

        [JsonProperty("line-width")]
        public DoubleFunction LineWidth { get; set; }

        [JsonProperty("line-gap-width")]
        public DoubleFunction LineGapWidth { get; set; }

        [JsonProperty("line-offset")]
        public DoubleFunction LineOffset { get; set; }

        [JsonProperty("line-blur")]
        public DoubleFunction LineBlur { get; set; }

        [JsonProperty("line-dasharray")]
        public List<double> LineDashArray { get; set; }

        [JsonProperty("line-pattern")]
        public string LinePattern { get; set; }
    }
}