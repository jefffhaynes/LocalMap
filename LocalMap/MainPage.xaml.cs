using System;
using System.Collections.Generic;
using System.Reflection;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Mapsui.Layers;
using BruTile.Predefined;
using Mapsui.Geometries;
using Mapsui.Projection;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Utilities;

namespace LocalMap
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            //MapControl.Map.Layers.Add(new TileLayer(new VectorTileSource(new GlobalSphericalMercator())));
            //MapControl.Map.Layers.Add(OpenStreetMap.CreateTileLayer());

        }


        //private ILayer CreateSvgLayer(Geoposition geoposition)
        //{
        //    return new MemoryLayer
        //    {
        //        Name = "Svg Layer",
        //        DataSource = CreateMemoryProviderWithDiverseSymbols(geoposition),
        //        Style = null
        //    };
        //}

        //public MemoryProvider CreateMemoryProviderWithDiverseSymbols(Geoposition geoposition)
        //{
        //    return new MemoryProvider(CreateSvgFeatures(geoposition));
        //}

        //private Features CreateSvgFeatures(Geoposition position)
        //{
        //    var features = new Features();
        //    var feature = new Feature
        //    {
        //        Geometry = SphericalMercator.FromLonLat(position.Coordinate.Longitude, position.Coordinate.Latitude),
        //        ["Label"] = "hello"
        //    };

        //    feature.Styles.Add(CreateSvgStyle("LocalMap.circle.svg", .25));
        //    features.Add(feature);
        //    return features;
        //}

        //private SymbolStyle CreateSvgStyle(string embeddedResourcePath, double scale)
        //{
        //    var bitmapId = GetBitmapIdForEmbeddedResource(embeddedResourcePath);
        //    return new SymbolStyle { BitmapId = bitmapId, SymbolType = SymbolType.Svg, SymbolScale = scale, SymbolOffset = new Offset(0.0, 0.5, true) };
        //}

        //private int GetBitmapIdForEmbeddedResource(string imagePath)
        //{
        //    var assembly = GetType().Assembly;
        //    var image = assembly.GetManifestResourceStream(imagePath);
        //    return BitmapRegistry.Instance.Register(image);
        //}
    }
}
