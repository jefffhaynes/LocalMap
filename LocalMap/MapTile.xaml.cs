using Windows.UI.Xaml.Controls;

namespace LocalMap
{
    public sealed partial class MapTile : UserControl
    {
        public MapTile()
        {
            this.InitializeComponent();
        }

        public int ZoomLevel { get; set; }

        public int X { get; set; }

        public int Y { get; set; }
    }
}
