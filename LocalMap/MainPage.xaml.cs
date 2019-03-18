using MapControl;
using MapTileLayer = MapControl.MapTileLayer;

namespace MapTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            
            MapControl.Center = new Location(0, 0);
            MapControl.MapLayer = new MapTileLayer(new DatabaseTileImageLoader())
            {
                MinZoomLevel = 0,
                MaxZoomLevel = 24
            };
        }
    }
}
