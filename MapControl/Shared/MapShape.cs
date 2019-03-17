// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

#if WINDOWS_UWP
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace MapControl
{
    /// <summary>
    /// Base class for MapPolyline and MapPolygon.
    /// </summary>
    public abstract partial class MapShape : IMapElement
    {
        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register(
            nameof(Location), typeof(Location), typeof(MapShape),
            new PropertyMetadata(null, (o, e) => ((MapShape)o).LocationPropertyChanged()));

        /// <summary>
        /// Gets or sets an optional Location to constrain the viewport position to the visible
        /// map viewport, as done for elements where the MapPanel.Location property is set.
        /// </summary>
        public Location Location
        {
            get => (Location)GetValue(LocationProperty);
            set => SetValue(LocationProperty, value);
        }

        private async void LocationPropertyChanged()
        {
            if (_parentMap != null)
            {
                await UpdateDataAsync();
            }
        }

        private MapBase _parentMap;

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

            return UpdateDataAsync();
        }

        private async void OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            await UpdateDataAsync();
        }

        protected abstract Task UpdateDataAsync();

        protected MapShape()
            : this(new PathGeometry())
        {
        }

        protected MapShape(Geometry data)
        {
            Data = data;

            MapPanel.InitMapElement(this);
        }

        protected Point LocationToPoint(Location location)
        {
            var point = _parentMap.MapProjection.LocationToPoint(location);

            if (point.Y == double.PositiveInfinity)
            {
                point.Y = 1e9;
            }
            else if (point.X == double.NegativeInfinity)
            {
                point.Y = -1e9;
            }

            return point;
        }

        protected Point LocationToViewportPoint(Location location)
        {
            return _parentMap.MapProjection.ViewportTransform.Transform(LocationToPoint(location));
        }

        protected double GetLongitudeOffset()
        {
            var longitudeOffset = 0d;

            if (_parentMap.MapProjection.IsNormalCylindrical && Location != null)
            {
                var viewportPosition = LocationToViewportPoint(Location);

                if (viewportPosition.X < 0d || viewportPosition.X > _parentMap.RenderSize.Width ||
                    viewportPosition.Y < 0d || viewportPosition.Y > _parentMap.RenderSize.Height)
                {
                    var nearestLongitude = Location.NearestLongitude(Location.Longitude, _parentMap.Center.Longitude);

                    longitudeOffset = nearestLongitude - Location.Longitude;
                }
            }

            return longitudeOffset;
        }
    }
}
