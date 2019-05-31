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
        public IExpression<double> BackgroundOpacity { get; set; }

        [JsonProperty("fill-antialias")]
        public bool? FillAntialias { get; set; }

        [JsonProperty("fill-opacity")]
        public IExpression<double> FillOpacity { get; set; }

        [JsonProperty("fill-color")]
        public ColorFunction FillColor { get; set; }

        [JsonProperty("fill-outline-color")]
        public ColorFunction FillOutlineColor { get; set; }

        [JsonProperty("fill-translate")]
        public List<double> FillTranslate { get; set; }

        [JsonProperty("fill-pattern")]
        public string FillPattern { get; set; }

        [JsonProperty("line-miter-limit")]
        public IExpression<double> LineMiterLimit { get; set; }

        [JsonProperty("line-round-limit")]
        public IExpression<double> LineRoundLimit { get; set; }

        [JsonProperty("line-opacity")]
        public IExpression<double> LineOpacity { get; set; }

        [JsonProperty("line-color")]
        public ColorFunction LineColor { get; set; }

        [JsonProperty("line-translate")]
        public List<double> LineTranslate { get; set; }

        [JsonProperty("line-width")]
        public IExpression<double> LineWidth { get; set; }

        [JsonProperty("line-gap-width")]
        public IExpression<double> LineGapWidth { get; set; }

        [JsonProperty("line-offset")]
        public IExpression<double> LineOffset { get; set; }

        [JsonProperty("line-blur")]
        public IExpression<double> LineBlur { get; set; }

        [JsonProperty("line-dasharray")]
        public List<float> LineDashArray { get; set; }

        [JsonProperty("line-pattern")]
        public string LinePattern { get; set; }

        [JsonProperty("text-color")]
        public ColorFunction TextColor { get; set; }

        [JsonProperty("text-halo-blur")]
        public IExpression<double> TextHaloBlur { get; set; }

        [JsonProperty("text-halo-color")]
        public ColorFunction TextHaloColor { get; set; }

        [JsonProperty("text-halo-width")]
        public IExpression<double> TextHaloWidth { get; set; }
    }
}