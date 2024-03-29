﻿using System.Collections.Generic;
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
        public StringFunction TextAnchor { get; set; }

        [JsonProperty("text-padding")]
        public DoubleFunction TextPadding { get; set; }

        [JsonProperty("text-transform")]
        public TextTransform? TextTransform { get; set; }

        [JsonProperty("text-offset")]
        public List<float> TextOffset { get; set; }

        [JsonProperty("max-text-width")]
        public DoubleFunction MaximumTextWidth { get; set; }

        [JsonProperty("line-cap")]
        public LineCap? LineCap { get; set; }

        [JsonProperty("line-join")]
        public LineJoin? LineJoin { get; set; }

        [JsonProperty("symbol-placement")]
        public StringFunction SymbolPlacement { get; set; }

        [JsonProperty("symbol-spacing")]
        public DoubleFunction SymbolSpacing { get; set; }

        [JsonProperty("icon-image")]
        public StringFunction IconImage { get; set; }
    }
}
