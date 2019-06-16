using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LocalMap
{
    public sealed partial class MapControl : UserControl
    {
        private const double MaxDegreesX = 180;
        private const double MinDegreesX = -MaxDegreesX;
        private static readonly double MaxDegreesY = Math.Atan(Math.Sinh(Math.PI)) * 180 / Math.PI;
        private static readonly double MinDegreesY = -MaxDegreesY;
        private static readonly double WidthDegrees = MaxDegreesX - MinDegreesX;
        private static readonly double HeightDegrees = MaxDegreesY - MinDegreesY;

        private double _degreesPerPixel;

        public MapControl()
        {
            InitializeComponent();
            ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY |
                               ManipulationModes.TranslateInertia;

            Loaded += OnLoadedAsync;
            SizeChanged += OnSizeChanged;
            ManipulationDelta += OnManipulationDelta;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateDegreesPerPixel();
        }

        private void UpdateDegreesPerPixel()
        {
            var widthDegreesPerPixel = WidthDegrees / ActualWidth;
            var heightDegreesPerPixel = HeightDegrees / ActualHeight;
            _degreesPerPixel = Math.Min(widthDegreesPerPixel, heightDegreesPerPixel) / Zoom;
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var delta = e.Delta.Translation;
            Center = new Geoposition(Center.Latitude + delta.X * _degreesPerPixel,
                Center.Longitude + delta.Y * _degreesPerPixel);
        }

        private async void OnLoadedAsync(object sender, RoutedEventArgs e)
        {
            await UpdateTilesAsync();
        }

        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(
            "Zoom", typeof(double), typeof(MapControl), new PropertyMetadata(1.0, ZoomPropertyChangedCallback));

        private async void ZoomPropertyChangedCallback(DependencyPropertyChangedEventArgs e)
        {
            var zoomCenter = new Vector3(_zoomCenter.Position.ToVector2(), 0);
            var scaleTransform = Matrix4x4.CreateScale((float) Zoom, zoomCenter);
            TransformMatrix = scaleTransform;

            UpdateDegreesPerPixel();
            await UpdateTilesAsync();
        }

        private async Task UpdateTilesAsync()
        {

            var zoomLevel = (int) Zoom;
            var tile = Tile.FromPosition(Center, zoomLevel);
            using (var stream = await DatabaseTileImageLoader.LoadTileAsync(tile))
            {
                var bitmap = new BitmapImage();
                bitmap.SetSource(stream);
                Image.Source = bitmap;

                var centerX = bitmap.PixelWidth / 2;
                var centerY = bitmap.PixelHeight / 2;
                Image.TransformMatrix = Matrix4x4.CreateScale(1.0f/(float)Math.Pow(2, zoomLevel), new Vector3(centerX, centerY, 0));
            }
        }

        //private GeoboundingBox GetGeobounds()
        //{
        //    var bounds = 
        //}

        private static void ZoomPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MapControl) d;
            control.ZoomPropertyChangedCallback(e);
        }

        public double Zoom
        {
            get => (double) GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }

        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register(
            "Center", typeof(Geoposition), typeof(MapControl),
            new PropertyMetadata(new Geoposition(0, 0), CenterPropertyChangedCallback));

        private void CenterPropertyChangedCallback(DependencyPropertyChangedEventArgs e)
        {
            var x = (float) (Center.Latitude / _degreesPerPixel);
            var y = (float) (Center.Longitude / _degreesPerPixel);
            var translate = Matrix4x4.CreateTranslation(x, y, 0);
            TransformMatrix = translate;
        }

        private static void CenterPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MapControl) d;
            control.CenterPropertyChangedCallback(e);
        }

        public Geoposition Center
        {
            get => (Geoposition) GetValue(CenterProperty);
            set => SetValue(CenterProperty, value);
        }


        private List<MapTile> _tiles = new List<MapTile>();


        //protected override Size ArrangeOverride(Size finalSize)
        //{
        //    return finalSize;
        //}

        private PointerPoint _zoomCenter;
        
        protected override void OnPointerWheelChanged(PointerRoutedEventArgs e)
        {
            _zoomCenter = e.GetCurrentPoint(this);
            var delta = _zoomCenter.Properties.MouseWheelDelta;

            var zoom = Zoom * (delta / 360.0 + 1);

            if (zoom < 1.0)
            {
                zoom = 1.0;
            }

            Zoom = zoom;
        }
    }
}
