using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MapboxStyle
{
    public class Style
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("layers")]
        public List<Layer> Layers { get; set; }

        public IEnumerable<Layer> GetLayers(string sourceLayer, double zoom)
        {
            return Layers.Where(layer => layer.SourceLayer != null &&
                                         layer.SourceLayer.Equals(sourceLayer, StringComparison.CurrentCultureIgnoreCase) &&
                                         (layer.MinimumZoom == null || layer.MinimumZoom <= zoom) &&
                                         (layer.MaximumZoom == null || layer.MaximumZoom >= zoom));
        }

        public Layer GetBackground(double zoom)
        {
            return Layers.FirstOrDefault(layer => EnumHelper<LayerType>.Parse(layer.Type.GetValue(zoom)) == LayerType.Background);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
