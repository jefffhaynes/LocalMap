using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media.Media3D;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;

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
            get => (Rect) GetValue(BoundsProperty);
            set => SetValue(BoundsProperty, value);
        }

        public Geobounds Boundary
        {
            get => (Geobounds) GetValue(BoundaryProperty);
            private set => SetValue(BoundaryProperty, value);
        }

        private readonly Dictionary<Tile, UIElement> _tiles = new Dictionary<Tile, UIElement>();

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

            UseLayoutRounding = false;
        }

        private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(this);
            var delta = point.Properties.MouseWheelDelta;

            var zoom = Zoom + delta / 1440.0;

            if (zoom < 1.0)
            {
                zoom = 1.0;
            }

            if (Math.Abs(Zoom - zoom) < double.Epsilon)
            {
                return;
            }

            var transform = (CompositeTransform) RenderTransform;

            var scale = Math.Pow(2, zoom - 1);

            var scaleDelta = scale - transform.ScaleX;

            transform.TranslateX += -point.Position.X * scaleDelta;
            transform.TranslateY += -point.Position.Y * scaleDelta;
            transform.ScaleX = scale;
            transform.ScaleY = scale;

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

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateTransform();
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var delta = e.Delta.Translation;

            var transform = (CompositeTransform) RenderTransform;
            transform.TranslateX += delta.X;
            transform.TranslateY += delta.Y;

            UpdateTransform();

            e.Handled = true;
        }

        private void ZoomPropertyChangedCallback(DependencyPropertyChangedEventArgs e)
        {
            UpdateTransform();
        }

        private int MapSize => 2 * TileSize;

        private int TilesAcross => (int) Math.Pow(2, (int) Zoom);

        private void UpdateTransform()
        {
            var element = (FrameworkElement) Window.Current.Content;
            var transform = element.TransformToVisual(this);

            var mapCenter = transform.TransformPoint(new Point(element.ActualWidth / 2, element.ActualHeight / 2));
            var mapBounds = transform.TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));

            //mapBounds.Intersect(new Rect(0, 0, ActualWidth, ActualHeight));
            Bounds = new Rect(mapBounds.X, mapBounds.Y, mapBounds.Width, mapBounds.Height);


            var geoTransform = new CompositeTransform
            {
                TranslateX = -WidthDegrees / 2,
                TranslateY = HeightDegrees / 2,
                ScaleX = WidthDegrees / MapSize,
                ScaleY = -HeightDegrees / MapSize
            };


            var center = geoTransform.TransformPoint(mapCenter);
            Center = new Geoposition(center.X, center.Y);
            //Center = Mercator.Project(mapCenter.ToVector2(), MapSize);

            var bounds = geoTransform.TransformBounds(mapBounds);
            Boundary = new Geobounds(bounds.X, bounds.Bottom, bounds.Width, bounds.Height);

            //Boundary = Mercator.Project(mapBounds, MapSize);

            UpdateTiles();
        }

        //public Geobounds Boundary { get; private set; }


        private void UpdateTiles()
        {
            var zoomLevel = (int) Zoom;

            var scale = 1 / Math.Pow(2, zoomLevel - 1);
            var tileSize = TileSize * scale;

            var left = (int) Math.Max(Bounds.Left / tileSize, 0);
            var right = (int) Math.Ceiling(Bounds.Right / tileSize);
            var top = (int) Math.Max(Bounds.Top / tileSize, 0);
            var bottom = (int) Math.Ceiling(Bounds.Bottom / tileSize);

            var max = TilesAcross - 1;

            left = Math.Clamp(left, 0, max);
            right = Math.Clamp(right, 0, max);
            top = Math.Clamp(top, 0, max);
            bottom = Math.Clamp(bottom, 0, max);

            for (int x = left; x <= right; x++)
            {
                for (int y = top; y <= bottom; y++)
                {
                    var tile = new Tile(x, y, zoomLevel);

                    if (!_tiles.ContainsKey(tile))
                    {
                        var canvas = new CanvasControl
                        {
                            DataContext = tile, Width = TileSize, Height = TileSize, UseLayoutRounding = false
                        };

                        canvas.Draw += CanvasOnDraw;

                        var box = new Viewbox {Child = canvas, DataContext = tile, UseLayoutRounding = false};

                        _tiles.Add(tile, box);

                        Children.Add(box);
                    }
                }
            }

            // TODO trim outside bounds
            var toRemove = _tiles.Where(kvp => kvp.Key.ZoomLevel != zoomLevel).ToList();

            foreach (var keyValuePair in toRemove)
            {
                Children.Remove(keyValuePair.Value);
                _tiles.Remove(keyValuePair.Key);

                ((CanvasControl)((Viewbox)keyValuePair.Value).Child).RemoveFromVisualTree();
            }
        }

        private async void CanvasOnDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var tile = (Tile)sender.DataContext;
            await DatabaseTileImageLoader.DrawTileAsync(tile, args.DrawingSession, TileSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (FrameworkElement child in Children)
            {
                var tile = (Tile) child.DataContext;
                var scale = 2.0 / Math.Pow(2, tile.ZoomLevel);
                var tileSize = TileSize * scale;

                var rect = new Rect(tile.X * tileSize, tile.Y * tileSize, tileSize, tileSize);
                child.Arrange(rect);

            }

            return finalSize;
        }

        private static void ZoomPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MapPanel)d;
            control.ZoomPropertyChangedCallback(e);
        }

        private void CenterPropertyChangedCallback(DependencyPropertyChangedEventArgs e)
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
