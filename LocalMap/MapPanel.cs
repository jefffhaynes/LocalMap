using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Mapsui;

namespace LocalMap
{
    public class MapPanel : Panel
    {
        private const double MaxDegreesX = 180;
        private const double MinDegreesX = -MaxDegreesX;
        private static readonly double MaxDegreesY = Math.Atan(Math.Sinh(Math.PI)) * 180 / Math.PI;
        private static readonly double MinDegreesY = -MaxDegreesY;
        private static readonly double WidthDegrees = MaxDegreesX - MinDegreesX;
        private static readonly double HeightDegrees = MaxDegreesY - MinDegreesY;

        private static readonly Geobounds MaxBounds = new Geobounds(MinDegreesX, MaxDegreesY, 
            MaxDegreesX - MinDegreesX,
            MaxDegreesY - MinDegreesY);

        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(
            "Zoom", typeof(double), typeof(MapPanel), new PropertyMetadata(1.0, ZoomPropertyChangedCallback));

        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register(
            "Center", typeof(Geoposition), typeof(MapPanel),
            new PropertyMetadata(new Geoposition(0, 0), CenterPropertyChangedCallback));

        //private double _degreesPerPixel;


        private readonly List<Tile> _tiles = new List<Tile>();

        private Vector2 _zoomCenter;
        //private double _widthDegreesPerPixel;
        //private double _heightDegreesPerPixel;

        public MapPanel()
        {
            ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY |
                               ManipulationModes.TranslateInertia;

            SizeChanged += OnSizeChanged;
            ManipulationDelta += OnManipulationDelta;
            PointerWheelChanged += OnPointerWheelChanged;

            Background = new SolidColorBrush(Colors.Black);
        }

        private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(this);
            var delta = point.Properties.MouseWheelDelta;

            var zoom = Zoom * (delta / 1440.0 + 1);

            if (zoom < 1.0)
            {
                zoom = 1.0;
            }

            Zoom = zoom;

            _zoomCenter = point.Position.ToVector2();
        }

        public double Zoom
        {
            get => (double)GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }

        public Geoposition Center
        {
            get => (Geoposition)GetValue(CenterProperty);
            set => SetValue(CenterProperty, value);
        }

        private async void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //UpdateDegreesPerPixel();
            UpdateTransform();
            await UpdateTilesAsync();
        }

        //private void UpdateDegreesPerPixel()
        //{
        //    _widthDegreesPerPixel = WidthDegrees / ActualWidth;
        //    _heightDegreesPerPixel = HeightDegrees / ActualHeight;
        //    _degreesPerPixel = Math.Min(_widthDegreesPerPixel, _heightDegreesPerPixel) / Zoom;
        //}

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var delta = e.Delta.Translation;

            Center = new Geoposition(
                Center.Longitude + delta.X * WidthDegreesPerPixel,
                Center.Latitude + delta.Y * HeightDegreesPerPixel);
        }

        private async void ZoomPropertyChangedCallback(DependencyPropertyChangedEventArgs e)
        {
            UpdateTransform();

            await UpdateTilesAsync();
        }

        private double MapSize => Math.Max(ActualWidth, ActualHeight);

        private double TileSize => MapSize / Math.Pow(2, (int) Zoom);

        private double WidthDegreesPerPixel => WidthDegrees / MapSize;

        private double HeightDegreesPerPixel => HeightDegrees / MapSize;

        private void UpdateTransform()
        {
            var x = (float)(Center.Longitude / WidthDegreesPerPixel);
            var y = (float)(Center.Latitude / HeightDegreesPerPixel);
            var center = new Vector2(x, y);
            var dx = (float)(MapSize - ActualWidth) / 2;
            var dy = (float)(MapSize - ActualHeight) / 2;
            var d = new Vector2(dx, dy);
            var translation = center - d;

            var zoomCenter = _zoomCenter;
            var scaleTransform = Matrix4x4.CreateScale((float) Zoom, new Vector3(zoomCenter, 0));
            var translationTransform = Matrix4x4.CreateTranslation(new Vector3(translation, 0));

            TransformMatrix = scaleTransform * translationTransform;
        }

        public Geobounds Boundary
        {
            get
            {
                var width = WidthDegreesPerPixel * ActualWidth;
                var height = HeightDegreesPerPixel * ActualHeight;
                var longitude = Center.Longitude - width / 2;
                var latitude = Center.Latitude + height / 2;
                var position = new Geoposition(longitude, latitude);
                return new Geobounds(position, width, height);
            }
        }

        private int _lastZoomLevel;

        private async Task UpdateTilesAsync()
        {
            var boundary = Boundary;

            var zoomLevel = (int)Zoom;
            var topLeft = Tile.FromPosition(boundary.TopLeft, zoomLevel).Bounded();
            var bottomRight = Tile.FromPosition(boundary.BottomRight, zoomLevel).Bounded();

            if (zoomLevel != _lastZoomLevel)
            {
                Children.Clear();
                _tiles.Clear();
            }

            //var tileWidth = ActualWidth / Math.Pow(2, (int)Zoom);


            for (int x = topLeft.X; x <= bottomRight.X; x++)
            {
                for (int y = topLeft.Y; y <= bottomRight.Y; y++)
                {
                    var tile = new Tile(x, y, zoomLevel);

                    if (!_tiles.Contains(tile))
                    {
                        //using (var stream = await DatabaseTileImageLoader.LoadTileAsync(tile))
                        {
                            //var bitmap = new BitmapImage();
                            //bitmap.SetSource(stream);

                            //var image = new Image
                            //{
                            //    Source = bitmap, DataContext = tile, Stretch = Stretch.UniformToFill
                            //};

                            //image.Arrange(new Rect(x * tileWidth, y * tileWidth, tileWidth, tileWidth));

                            //var centerX = bitmap.PixelWidth / 2;
                            //var centerY = bitmap.PixelHeight / 2;
                            //image.TransformMatrix = Matrix4x4.CreateScale(1.0f / (float)Math.Pow(2, zoomLevel),
                            //    new Vector3(centerX, centerY, 0));

                            var image = new TextBlock();
                            image.TextAlignment = TextAlignment.Center;
                            image.HorizontalTextAlignment = TextAlignment.Center;
                            image.HorizontalAlignment = HorizontalAlignment.Stretch;
                            image.VerticalAlignment = VerticalAlignment.Stretch;
                            image.Text = $"{tile.X}, {tile.Y}";
                            image.Width = TileSize;
                            image.Height = TileSize;

                            var border = new Border();
                            border.Background = new SolidColorBrush(Colors.Blue);
                            border.BorderBrush = new SolidColorBrush(Colors.Red);
                            border.BorderThickness = new Thickness(1);
                            border.Child = image;

                            border.DataContext = tile;
                            Children.Add(border);
                        }

                        _tiles.Add(tile);
                    }

                }
            }

            InvalidateArrange();

            _lastZoomLevel = zoomLevel;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var tileWidth = ActualWidth / Math.Pow(2, (int)Zoom);

            foreach (FrameworkElement child in Children)
            {
                var tile = (Tile) child.DataContext;
                var rect = new Rect(tile.X * tileWidth, tile.Y * tileWidth, tileWidth, tileWidth);
                child.Arrange(rect);
            }

            return finalSize;
        }

        private static void ZoomPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MapPanel)d;
            control.ZoomPropertyChangedCallback(e);
        }

        private async void CenterPropertyChangedCallback(DependencyPropertyChangedEventArgs e)
        {
            UpdateTransform();
            await UpdateTilesAsync();
        }

        private static void CenterPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MapPanel)d;
            control.CenterPropertyChangedCallback(e);
        }
    }
}
