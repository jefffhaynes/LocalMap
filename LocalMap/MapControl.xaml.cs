using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

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

        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(
            "Zoom", typeof(double), typeof(MapControl), new PropertyMetadata(1.0, ZoomPropertyChangedCallback));

        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register(
            "Center", typeof(Geoposition), typeof(MapControl),
            new PropertyMetadata(new Geoposition(0, 0), CenterPropertyChangedCallback));

        private double _degreesPerPixel;
        private Matrix4x4 _scaleTransform = Matrix4x4.Identity;


        private List<MapTile> _tiles = new List<MapTile>();
        private Matrix4x4 _translationTransform = Matrix4x4.Identity;

        private PointerPoint _zoomCenter;
        private double _widthDegreesPerPixel;
        private double _heightDegreesPerPixel;

        public MapControl()
        {
            InitializeComponent();
            ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY |
                               ManipulationModes.TranslateInertia;

            Loaded += OnLoadedAsync;
            SizeChanged += OnSizeChanged;
            ManipulationDelta += OnManipulationDelta;
        }

        public double Zoom
        {
            get => (double) GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }

        public Geoposition Center
        {
            get => (Geoposition) GetValue(CenterProperty);
            set => SetValue(CenterProperty, value);
        }

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

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateDegreesPerPixel();
        }

        private void UpdateDegreesPerPixel()
        {
            _widthDegreesPerPixel = WidthDegrees / ActualWidth;
            _heightDegreesPerPixel = HeightDegrees / ActualHeight;
            _degreesPerPixel = Math.Min(_widthDegreesPerPixel, _heightDegreesPerPixel) / Zoom;
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var delta = e.Delta.Translation;
            Center = new Geoposition(Center.Longitude + delta.X * _degreesPerPixel, Center.Latitude + delta.Y * _degreesPerPixel);
        }

        private async void OnLoadedAsync(object sender, RoutedEventArgs e)
        {
            await UpdateTilesAsync();
        }

        private async void ZoomPropertyChangedCallback(DependencyPropertyChangedEventArgs e)
        {
            var zoomCenter = new Vector3(_zoomCenter.Position.ToVector2(), 0);
            _scaleTransform = Matrix4x4.CreateScale((float) Zoom, zoomCenter);

            UpdateDegreesPerPixel();
            UpdateTransform();

            await UpdateTilesAsync();
        }

        private void UpdateTransform()
        {
            TransformMatrix = _scaleTransform * _translationTransform;
        }

        public Geobounds Boundary
        {
            get
            {
                var width = _widthDegreesPerPixel * ActualWidth;
                var height = _heightDegreesPerPixel * ActualHeight;
                return new Geobounds(Center, width, height);
            }
        }

        private async Task UpdateTilesAsync()
        {
            var boundary = Boundary;

            var zoomLevel = (int)Zoom;
            var topLeft = Tile.FromPosition(boundary.TopLeft, zoomLevel).Bounded();
            var bottomRight = Tile.FromPosition(boundary.BottomRight, zoomLevel).Bounded();

            
            for (int x = topLeft.X; x < bottomRight.X; x++)
            {
                for (int y = topLeft.Y; y < bottomRight.Y; y++)
                {

                }
            }

            var tile = Tile.FromPosition(Center, zoomLevel);
            using (var stream = await DatabaseTileImageLoader.LoadTileAsync(tile))
            {
                var bitmap = new BitmapImage();
                bitmap.SetSource(stream);
                Image.Source = bitmap;

                var centerX = bitmap.PixelWidth / 2;
                var centerY = bitmap.PixelHeight / 2;
                Image.TransformMatrix = Matrix4x4.CreateScale(1.0f / (float) Math.Pow(2, zoomLevel),
                    new Vector3(centerX, centerY, 0));
            }
        }

        private static void ZoomPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MapControl) d;
            control.ZoomPropertyChangedCallback(e);
        }

        private void CenterPropertyChangedCallback(DependencyPropertyChangedEventArgs e)
        {
            var x = (float) (Center.Longitude / _degreesPerPixel);
            var y = (float) (Center.Latitude / _degreesPerPixel);
            _translationTransform = Matrix4x4.CreateTranslation(x, y, 0);
            UpdateTransform();
        }

        private static void CenterPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MapControl) d;
            control.CenterPropertyChangedCallback(e);
        }
    }
}