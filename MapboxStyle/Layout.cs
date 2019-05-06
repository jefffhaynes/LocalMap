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

        [JsonProperty("line-cap")]
        public LineCap? LineCap { get; set; }

        [JsonProperty("line-join")]
        public LineJoin? LineJoin { get; set; }

        [JsonProperty("line-dasharray")]
        public List<float> LineDashArray { get; set; }
    }
}