// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
#endif

namespace MapControl
{
    public interface ITileImageLoader
    {
        Task LoadTilesAsync(MapTileLayer tileLayer);
    }

    /// <summary>
    ///     Fills the map viewport with map tiles from a TileSource.
    /// </summary>
    public class MapTileLayer : Panel, IMapLayer
    {
        public static readonly DependencyProperty TileSourceProperty = DependencyProperty.Register(
            nameof(TileSource), typeof(TileSource), typeof(MapTileLayer),
            new PropertyMetadata(null, (o, e) => ((MapTileLayer) o).TileSourcePropertyChanged()));

        public static readonly DependencyProperty SourceNameProperty = DependencyProperty.Register(
            nameof(SourceName), typeof(string), typeof(MapTileLayer), new PropertyMetadata(null));

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            nameof(Description), typeof(string), typeof(MapTileLayer), new PropertyMetadata(null));

        public static readonly DependencyProperty ZoomLevelOffsetProperty = DependencyProperty.Register(
            nameof(ZoomLevelOffset), typeof(double), typeof(MapTileLayer),
            new PropertyMetadata(0d, async (o, e) => await ((MapTileLayer) o).UpdateTileGridAsync()));

        public static readonly DependencyProperty MinZoomLevelProperty = DependencyProperty.Register(
            nameof(MinZoomLevel), typeof(int), typeof(MapTileLayer), new PropertyMetadata(0));

        public static readonly DependencyProperty MaxZoomLevelProperty = DependencyProperty.Register(
            nameof(MaxZoomLevel), typeof(int), typeof(MapTileLayer), new PropertyMetadata(18));

        public static readonly DependencyProperty UpdateIntervalProperty = DependencyProperty.Register(
            nameof(UpdateInterval), typeof(TimeSpan), typeof(MapTileLayer),
            new PropertyMetadata(TimeSpan.FromSeconds(0.2),
                (o, e) => ((MapTileLayer) o)._updateTimer.Interval = (TimeSpan) e.NewValue));

        public static readonly DependencyProperty UpdateWhileViewportChangingProperty = DependencyProperty.Register(
            nameof(UpdateWhileViewportChanging), typeof(bool), typeof(MapTileLayer), new PropertyMetadata(true));

        public static readonly DependencyProperty MapBackgroundProperty = DependencyProperty.Register(
            nameof(MapBackground), typeof(Brush), typeof(MapTileLayer), new PropertyMetadata(null));

        public static readonly DependencyProperty MapForegroundProperty = DependencyProperty.Register(
            nameof(MapForeground), typeof(Brush), typeof(MapTileLayer), new PropertyMetadata(null));

        private readonly DispatcherTimer _updateTimer;
        private MapBase _parentMap;

        public MapTileLayer()
            : this(new TileImageLoader())
        {
        }

        public MapTileLayer(ITileImageLoader tileImageLoader)
        {
            IsHitTestVisible = false;
            RenderTransform = new MatrixTransform();
            TileImageLoader = tileImageLoader;

            _updateTimer = new DispatcherTimer {Interval = UpdateInterval};
            _updateTimer.Tick += async (s, e) => await UpdateTileGridAsync();

            MapPanel.InitMapElement(this);
        }

        public ITileImageLoader TileImageLoader { get; }

        public TileGrid TileGrid { get; private set; }

        public IReadOnlyCollection<Tile> Tiles { get; private set; } = new List<Tile>();

        /// <summary>
        ///     Provides map tile URIs or images.
        /// </summary>
        public TileSource TileSource
        {
            get => (TileSource) GetValue(TileSourceProperty);
            set => SetValue(TileSourceProperty, value);
        }

        /// <summary>
        ///     Name of the TileSource. Used as component of a tile cache key.
        /// </summary>
        public string SourceName
        {
            get => (string) GetValue(SourceNameProperty);
            set => SetValue(SourceNameProperty, value);
        }

        /// <summary>
        ///     Description of the MapTileLayer. Used to display copyright information on top of the map.
        /// </summary>
        public string Description
        {
            get => (string) GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        /// <summary>
        ///     Adds an offset to the Map's ZoomLevel for a relative scale between the Map and the MapTileLayer.
        /// </summary>
        public double ZoomLevelOffset
        {
            get => (double) GetValue(ZoomLevelOffsetProperty);
            set => SetValue(ZoomLevelOffsetProperty, value);
        }

        /// <summary>
        ///     Minimum zoom level supported by the MapTileLayer.
        /// </summary>
        public int MinZoomLevel
        {
            get => (int) GetValue(MinZoomLevelProperty);
            set => SetValue(MinZoomLevelProperty, value);
        }

        /// <summary>
        ///     Maximum zoom level supported by the MapTileLayer.
        /// </summary>
        public int MaxZoomLevel
        {
            get => (int) GetValue(MaxZoomLevelProperty);
            set => SetValue(MaxZoomLevelProperty, value);
        }

        /// <summary>
        ///     Minimum time interval between tile updates.
        /// </summary>
        public TimeSpan UpdateInterval
        {
            get => (TimeSpan) GetValue(UpdateIntervalProperty);
            set => SetValue(UpdateIntervalProperty, value);
        }

        /// <summary>
        ///     Controls if tiles are updated while the viewport is still changing.
        /// </summary>
        public bool UpdateWhileViewportChanging
        {
            get => (bool) GetValue(UpdateWhileViewportChangingProperty);
            set => SetValue(UpdateWhileViewportChangingProperty, value);
        }

        /// <summary>
        ///     Optional background brush. Sets MapBase.Background if not null and the MapTileLayer is the base map layer.
        /// </summary>
        public Brush MapBackground
        {
            get => (Brush) GetValue(MapBackgroundProperty);
            set => SetValue(MapBackgroundProperty, value);
        }

        /// <summary>
        ///     Optional foreground brush. Sets MapBase.Foreground if not null and the MapTileLayer is the base map layer.
        /// </summary>
        public Brush MapForeground
        {
            get => (Brush) GetValue(MapForegroundProperty);
            set => SetValue(MapForegroundProperty, value);
        }

        public MapBase GetParentMap() => _parentMap;

        public Task SetParentMapAsync(MapBase map)
        {
            if (_parentMap != null)
            {
                _parentMap.ViewportChanged -= OnViewportChanged;
            }

            _parentMap = map;

            if (_parentMap != null)
            {
                _parentMap.ViewportChanged += OnViewportChanged;
            }

            return UpdateTileGridAsync();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            foreach (var tile in Tiles)
            {
                tile.Image.Measure(availableSize);
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (TileGrid != null)
            {
                foreach (var tile in Tiles)
                {
                    var tileSize = MapProjection.TileSize << (TileGrid.ZoomLevel - tile.ZoomLevel);
                    var x = tileSize * tile.X - MapProjection.TileSize * TileGrid.XMin;
                    var y = tileSize * tile.Y - MapProjection.TileSize * TileGrid.YMin;

                    tile.Image.Width = tileSize;
                    tile.Image.Height = tileSize;
                    tile.Image.Arrange(new Rect(x, y, tileSize, tileSize));
                }
            }

            return finalSize;
        }

        protected virtual async Task UpdateTileGridAsync()
        {
            _updateTimer.Stop();

            if (_parentMap != null && _parentMap.MapProjection.IsWebMercator)
            {
                var tileGrid = GetTileGrid();

                if (!tileGrid.Equals(TileGrid))
                {
                    TileGrid = tileGrid;
                    SetRenderTransform();
                    await UpdateTilesAsync();
                }
            }
            else
            {
                TileGrid = null;
                await UpdateTilesAsync();
            }
        }

        private async void TileSourcePropertyChanged()
        {
            if (TileGrid != null)
            {
                Tiles = new List<Tile>();
                await UpdateTilesAsync();
            }
        }

        private async void OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            if (TileGrid == null || e.ProjectionChanged || Math.Abs(e.LongitudeOffset) > 180d)
            {
                await UpdateTileGridAsync(); // update immediately when projection has changed or center has moved across 180° longitude
            }
            else
            {
                SetRenderTransform();

                if (_updateTimer.IsEnabled && !UpdateWhileViewportChanging)
                {
                    _updateTimer.Stop(); // restart
                }

                if (!_updateTimer.IsEnabled)
                {
                    _updateTimer.Start();
                }
            }
        }

        private TileGrid GetTileGrid()
        {
            var tileZoomLevel = Math.Max(0, (int) Math.Round(_parentMap.ZoomLevel + ZoomLevelOffset));
            var tileScale = (double) (1 << tileZoomLevel);
            var scale = tileScale / (Math.Pow(2d, _parentMap.ZoomLevel) * MapProjection.TileSize);
            var tileCenter = new Point(tileScale * (0.5 + _parentMap.Center.Longitude / 360d),
                tileScale * (0.5 - WebMercatorProjection.LatitudeToY(_parentMap.Center.Latitude) / 360d));
            var viewCenter = new Point(_parentMap.RenderSize.Width / 2d, _parentMap.RenderSize.Height / 2d);

            var transform = new MatrixTransform
            {
                Matrix = MapProjection.CreateTransformMatrix(viewCenter, scale, -_parentMap.Heading, tileCenter)
            };

            var bounds =
                transform.TransformBounds(new Rect(0d, 0d, _parentMap.RenderSize.Width, _parentMap.RenderSize.Height));

            return new TileGrid(tileZoomLevel,
                (int) Math.Floor(bounds.X), (int) Math.Floor(bounds.Y),
                (int) Math.Floor(bounds.X + bounds.Width), (int) Math.Floor(bounds.Y + bounds.Height));
        }

        private void SetRenderTransform()
        {
            var tileScale = (double) (1 << TileGrid.ZoomLevel);
            var scale = Math.Pow(2d, _parentMap.ZoomLevel) / tileScale;
            var tileCenter = new Point(tileScale * (0.5 + _parentMap.Center.Longitude / 360d),
                tileScale * (0.5 - WebMercatorProjection.LatitudeToY(_parentMap.Center.Latitude) / 360d));
            var tileOrigin = new Point(MapProjection.TileSize * (tileCenter.X - TileGrid.XMin),
                MapProjection.TileSize * (tileCenter.Y - TileGrid.YMin));
            var viewCenter = new Point(_parentMap.RenderSize.Width / 2d, _parentMap.RenderSize.Height / 2d);

            ((MatrixTransform) RenderTransform).Matrix =
                MapProjection.CreateTransformMatrix(tileOrigin, scale, _parentMap.Heading, viewCenter);
        }

        private Task UpdateTilesAsync()
        {
            var newTiles = new List<Tile>();

            if (_parentMap != null && TileGrid != null)
            {
                var maxZoomLevel = Math.Min(TileGrid.ZoomLevel, MaxZoomLevel);
                var minZoomLevel = MinZoomLevel;

                if (minZoomLevel < maxZoomLevel && _parentMap.MapLayer != this)
                {
                    minZoomLevel = maxZoomLevel; // do not load lower level tiles if this is note a "base" layer
                }

                for (var zoom = minZoomLevel; zoom <= maxZoomLevel; zoom++)
                {
                    var zoomLevel = Math.Min(TileGrid.ZoomLevel, 14);
                    var limitedZoom = Math.Min(zoom, 14);

                    var tileSize = 1 << (zoomLevel - limitedZoom);
                    //var tileSize = 1 << (TileGrid.ZoomLevel - z);
                    var x1 = (int) Math.Floor((double) TileGrid.XMin / tileSize); // may be negative
                    var x2 = TileGrid.XMax / tileSize;
                    var y1 = Math.Max(TileGrid.YMin / tileSize, 0);
                    var y2 = Math.Min(TileGrid.YMax / tileSize, (1 << limitedZoom) - 1);

                    for (var y = y1; y <= y2; y++)
                    for (var x = x1; x <= x2; x++)
                    {
                        var tile = Tiles.FirstOrDefault(t => t.ZoomLevel == zoom && t.X == x && t.Y == y);

                        if (tile == null)
                        {
                            tile = new Tile(zoom, x, y);

                            var equivalentTile = Tiles.FirstOrDefault(
                                t => t.ZoomLevel == zoom && t.XIndex == tile.XIndex && t.Y == y && t.Image.Source != null);

                            if (equivalentTile != null)
                            {
                                tile.SetImage(equivalentTile.Image.Source, false); // no fade-in animation
                            }
                        }

                        newTiles.Add(tile);
                    }
                }
            }

            Tiles = newTiles;

            Children.Clear();

            foreach (var tile in Tiles)
            {
                Children.Add(tile.Image);
            }

            return TileImageLoader.LoadTilesAsync(this);
        }
    }
}