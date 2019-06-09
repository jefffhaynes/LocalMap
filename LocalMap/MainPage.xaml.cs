using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Mapsui.Layers;
using BruTile.Predefined;

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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            MapControl.Map.Layers.Add(new TileLayer(new VectorTileSource(new GlobalSphericalMercator())));
        }
    }
}
