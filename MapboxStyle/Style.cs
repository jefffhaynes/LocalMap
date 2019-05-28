using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MapboxStyle
{
    public class Style
    {
        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("layers")]
        public List<Layer> Layers { get; set; }

        public IEnumerable<Layer> GetLayers(string sourceLayer, double zoom)
        {
            return Layers.Where(layer => layer.SourceLayer != null &&
                                         layer.SourceLayer.Equals(sourceLayer, StringComparison.CurrentCultureIgnoreCase) &&
                                         (layer.MinimumZoom == null || layer.MinimumZoom < zoom) &&
                                         (layer.MaximumZoom == null || layer.MaximumZoom > zoom));
        }

        public Layer Background
        {
            get { return Layers.FirstOrDefault(layer => layer.Type == LayerType.Background); }
        }
    }
}
