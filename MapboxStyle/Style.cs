using System.Collections.Generic;
using Newtonsoft.Json;

namespace MapboxStyle
{
    public class Style
    {
        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("layers")]
        public List<Layer> Layers { get; set; }
    }
}
