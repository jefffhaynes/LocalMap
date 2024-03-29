﻿using System.Collections.Generic;
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
        public BoolFunction FillAntialias { get; set; }

        [JsonProperty("fill-opacity")]
        public DoubleFunction FillOpacity { get; set; }

        [JsonProperty("fill-color")]
        public ColorFunction FillColor { get; set; }

        [JsonProperty("fill-outline-color")]
        public ColorFunction FillOutlineColor { get; set; }

        //[JsonProperty("fill-translate")]
        //public List<double> FillTranslate { get; set; }

        [JsonProperty("fill-pattern")]
        public StringFunction FillPattern { get; set; }

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
        public List<float> LineDashArray { get; set; }

        [JsonProperty("line-pattern")]
        public string LinePattern { get; set; }

        [JsonProperty("text-color")]
        public ColorFunction TextColor { get; set; }

        [JsonProperty("text-halo-blur")]
        public DoubleFunction TextHaloBlur { get; set; }

        [JsonProperty("text-halo-color")]
        public ColorFunction TextHaloColor { get; set; }

        [JsonProperty("text-halo-width")]
        public DoubleFunction TextHaloWidth { get; set; }
    }
}