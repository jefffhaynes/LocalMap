using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private const int TileSize = 256;

        private static readonly Geobounds MaxBounds = new Geobounds(MinDegreesX, MaxDegreesY, 
            MaxDegreesX - MinDegreesX,
            MaxDegreesY - MinDegreesY);

        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(
            "Zoom", typeof(double), typeof(MapPanel), new PropertyMetadata(1.0, ZoomPropertyChangedCallback));

        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register(
            "Center", typeof(Geoposition), typeof(MapPanel),
            new PropertyMetadata(new Geoposition(0, 0), CenterPropertyChangedCallback));

        public static readonly DependencyProperty BoundaryProperty = DependencyProperty.Register(
            "Boundary", typeof(Geobounds), typeof(MapPanel), new PropertyMetadata(default(Geobounds)));

        public static readonly DependencyProperty BoundsProperty = DependencyProperty.Register(
            "Bounds", typeof(Rect), typeof(MapPanel), new PropertyMetadata(default(Rect)));

        public Rect Bounds
        {
            get { return (Rect) GetValue(BoundsProperty); }
            set { SetValue(BoundsProperty, value); }
        }

        public Geobounds Boundary
        {
            get => (Geobounds) GetValue(BoundaryProperty);
            private set => SetValue(BoundaryProperty, value);
        }

        private readonly List<Tile> _tiles = new List<Tile>();

        //private Vector2 _zoomCenter;

        public MapPanel()
        {
            ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY |
                               ManipulationModes.TranslateInertia;

            SizeChanged += OnSizeChanged;
            ManipulationDelta += OnManipulationDelta;
            PointerWheelChanged += OnPointerWheelChanged;

            Background = new SolidColorBrush(Colors.Black);


            var mapOffsetX = (float)(MapSize - ActualWidth) / 2;
            var mapOffsetY = (float)(MapSize - ActualHeight) / 2;

            RenderTransform = new CompositeTransform
            {
                TranslateX = mapOffsetX,
                TranslateY = mapOffsetY
            };

            Children.Add(new Border{BorderBrush = new SolidColorBrush(Colors.Purple), BorderThickness = new Thickness(3)});
        }

        private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(this);
            var delta = point.Properties.MouseWheelDelta;

            var zoom = Zoom * (delta / (1440.0 * 4) + 1);

            if (zoom < 1.0)
            {
                zoom = 1.0;
            }

            if (Math.Abs(Zoom - zoom) < double.Epsilon)
            {
                return;
            }

            var transform = (CompositeTransform) RenderTransform;

            var zoomDelta = zoom - Zoom;

            transform.TranslateX += -point.Position.X * zoomDelta;
            transform.TranslateY += -point.Position.Y * zoomDelta;
            transform.ScaleX = zoom;
            transform.ScaleY = zoom;

            Zoom = zoom;

            e.Handled = true;
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

        //private Vector2 _position;

        private async void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var delta = e.Delta.Translation;
            //_position += delta.ToVector2();

            var transform = (CompositeTransform) RenderTransform;
            transform.TranslateX += delta.X;
            transform.TranslateY += delta.Y;

            UpdateTransform();

            await UpdateTilesAsync();

            e.Handled = true;
        }

        private async void ZoomPropertyChangedCallback(DependencyPropertyChangedEventArgs e)
        {
            UpdateTransform();

            await UpdateTilesAsync();
        }

        private int MapSize => 2 * TileSize;

        private int TilesAcross => (int) Math.Pow(2, (int) Zoom);


        private void UpdateTransform()
        {
            var scale = Math.Pow(2, Zoom);

            var element = (FrameworkElement) Window.Current.Content;
            var transform = element.TransformToVisual(this);

            var mapCenter = transform.TransformPoint(new Point(element.ActualWidth / 2, element.ActualHeight / 2));
            var mapBounds = transform.TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));

            var geoTransform = new CompositeTransform
            {
                TranslateX = -WidthDegrees / 2,
                TranslateY = HeightDegrees / 2,
                ScaleX = WidthDegrees / MapSize,
                ScaleY = -HeightDegrees / MapSize
            };

            //mapBounds.Intersect(new Rect(0, 0, ActualWidth, ActualHeight));
            Bounds = new Rect(mapBounds.X, mapBounds.Y, mapBounds.Width * scale, mapBounds.Height * scale);

            var center = geoTransform.TransformPoint(mapCenter);
            Center = new Geoposition(center.X, center.Y);
            //Center = Mercator.Project(mapCenter.ToVector2(), MapSize);

            var bounds = geoTransform.TransformBounds(mapBounds);
            Boundary = new Geobounds(bounds.X, bounds.Bottom, bounds.Width, bounds.Height);

            //Boundary = Mercator.Project(mapBounds, MapSize);

        }

        //public Geobounds Boundary { get; private set; }

        private int _lastZoomLevel;

        private async Task UpdateTilesAsync()
        {
            //var boundary = Boundary;

            var zoomLevel = (int)Zoom;
            //var topLeft = Tile.FromPosition(Boundary.TopLeft, zoomLevel);
            //var bottomRight = Tile.FromPosition(Boundary.BottomRight, zoomLevel);

            var max = TilesAcross - 1;

            //var left = (int)Math.Clamp(Bounds.Left, 0, ActualWidth);
            //var right = (int)Math.Clamp(Bounds.Right, 0, ActualWidth);
            //var top = (int)Math.Clamp(Bounds.Top, 0, ActualHeight);
            //var bottom = (int)Math.Clamp(Bounds.Bottom, 0, ActualHeight);

            var left = Math.Max(Bounds.Left / TileSize, 0);
            var right = Math.Ceiling(Bounds.Right / TileSize);
            var top = Math.Max(Bounds.Top / TileSize, 0);
            var bottom = Math.Ceiling(Bounds.Bottom / TileSize);
            
            left = Math.Clamp(left, 0, max);
            right = Math.Clamp(right, 0, max);
            top = Math.Clamp(top, 0, max);
            bottom = Math.Clamp(bottom, 0, max);

            var tiles = new List<Tile>();

            for (int x = (int) left; x <= right; x++)
            {
                for (int y = (int) top; y <= bottom; y++)
                {
                    var tile = new Tile(x, y, zoomLevel);

                    if (!_tiles.Contains(tile))
                    {
                        tiles.Add(tile);
                    }
                }
            }

            var fetchTasks = tiles.Select(FetchAsync);
            var elements = await Task.WhenAll(fetchTasks);

            foreach (var uiElement in elements)
            {
                if (uiElement != null)
                {
                    Children.Add(uiElement);
                }
            }

            _tiles.AddRange(tiles);

            if (zoomLevel != _lastZoomLevel)
            {
                _tiles.RemoveAll(tile => tile.ZoomLevel == _lastZoomLevel);

                var toRemove = new List<UIElement>();

                foreach (FrameworkElement child in Children)
                {
                    var tile = (Tile)child.DataContext;
                    if (tile?.ZoomLevel == _lastZoomLevel)
                    {
                        toRemove.Add(child);
                    }
                }

                foreach (var uiElement in toRemove)
                {
                    Children.Remove(uiElement);
                }
            }

            //InvalidateArrange();

            _lastZoomLevel = zoomLevel;
        }

        private async Task<UIElement> FetchAsync(Tile tile)
        {
            using (var stream = await DatabaseTileImageLoader.LoadTileAsync(tile))
            {
                if (stream == null)
                {
                    return null;
                }

                var bitmap = new BitmapImage();
                bitmap.SetSource(stream);

                var image = new Image
                {
                    Source = bitmap,
                    DataContext = tile,
                    Stretch = Stretch.Fill
                };

                var max = Math.Pow(2, tile.ZoomLevel) - 1;
                if (tile.X > max || tile.Y > max)
                {
                    return null;
                }

                //var image = new TextBlock();
                //image.TextAlignment = TextAlignment.Left;
                //image.HorizontalTextAlignment = TextAlignment.Left;
                //image.HorizontalAlignment = HorizontalAlignment.Stretch;
                //image.VerticalAlignment = VerticalAlignment.Stretch;
                //image.Text = $"{tile.X}, {tile.Y} ({tile.ZoomLevel})";
                //image.Width = TileSize;
                //image.Height = TileSize;

                var border = new Border();
                border.Background = new SolidColorBrush(Colors.Blue);
                border.BorderBrush = new SolidColorBrush(Colors.Red);
                border.BorderThickness = new Thickness(1 / Zoom);
                border.Child = image;

                border.DataContext = tile;

                return border;
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var scale = 2.0 / TilesAcross;
            var tileSize = TileSize * scale;

            foreach (FrameworkElement child in Children)
            {
                var tile = (Tile) child.DataContext;

                if (tile == null)
                {
                    var rect = new Rect(0, 0, MapSize, MapSize);
                    child.Arrange(rect);
                }
                else
                {
                    var rect = new Rect(tile.X * tileSize, tile.Y * tileSize, tileSize, tileSize);
                    child.Arrange(rect);
                }
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
            //UpdateTransform();
            //await UpdateTilesAsync();
        }

        private static void CenterPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MapPanel)d;
            control.CenterPropertyChangedCallback(e);
        }
    }
}
