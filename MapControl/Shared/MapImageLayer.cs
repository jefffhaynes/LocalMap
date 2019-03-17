// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
#endif

namespace MapControl
{
    /// <summary>
    ///     Map image layer. Fills the entire viewport with a map image, e.g. provided by a Web Map Service (WMS).
    ///     The image must be provided by the abstract GetImageAsync method.
    /// </summary>
    public abstract class MapImageLayer : MapPanel, IMapLayer
    {
        public static readonly DependencyProperty MinLatitudeProperty = DependencyProperty.Register(
            nameof(MinLatitude), typeof(double), typeof(MapImageLayer), new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty MaxLatitudeProperty = DependencyProperty.Register(
            nameof(MaxLatitude), typeof(double), typeof(MapImageLayer), new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty MinLongitudeProperty = DependencyProperty.Register(
            nameof(MinLongitude), typeof(double), typeof(MapImageLayer), new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty MaxLongitudeProperty = DependencyProperty.Register(
            nameof(MaxLongitude), typeof(double), typeof(MapImageLayer), new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty MaxBoundingBoxWidthProperty = DependencyProperty.Register(
            nameof(MaxBoundingBoxWidth), typeof(double), typeof(MapImageLayer), new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty RelativeImageSizeProperty = DependencyProperty.Register(
            nameof(RelativeImageSize), typeof(double), typeof(MapImageLayer), new PropertyMetadata(1d));

        public static readonly DependencyProperty UpdateIntervalProperty = DependencyProperty.Register(
            nameof(UpdateInterval), typeof(TimeSpan), typeof(MapImageLayer),
            new PropertyMetadata(TimeSpan.FromSeconds(0.2),
                (o, e) => ((MapImageLayer) o)._updateTimer.Interval = (TimeSpan) e.NewValue));

        public static readonly DependencyProperty UpdateWhileViewportChangingProperty = DependencyProperty.Register(
            nameof(UpdateWhileViewportChanging), typeof(bool), typeof(MapImageLayer), new PropertyMetadata(false));

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            nameof(Description), typeof(string), typeof(MapImageLayer), new PropertyMetadata(null));

        public static readonly DependencyProperty MapBackgroundProperty = DependencyProperty.Register(
            nameof(MapBackground), typeof(Brush), typeof(MapImageLayer), new PropertyMetadata(null));

        public static readonly DependencyProperty MapForegroundProperty = DependencyProperty.Register(
            nameof(MapForeground), typeof(Brush), typeof(MapImageLayer), new PropertyMetadata(null));

        private readonly DispatcherTimer _updateTimer;
        private BoundingBox _boundingBox;
        private bool _updateInProgress;

        protected MapImageLayer()
        {
            Children.Add(new Image {Opacity = 0d, Stretch = Stretch.Fill});
            Children.Add(new Image {Opacity = 0d, Stretch = Stretch.Fill});

            _updateTimer = new DispatcherTimer {Interval = UpdateInterval};
            _updateTimer.Tick += async (s, e) => await UpdateImageAsync();
        }

        /// <summary>
        ///     Optional minimum latitude value. Default is NaN.
        /// </summary>
        public double MinLatitude
        {
            get => (double) GetValue(MinLatitudeProperty);
            set => SetValue(MinLatitudeProperty, value);
        }

        /// <summary>
        ///     Optional maximum latitude value. Default is NaN.
        /// </summary>
        public double MaxLatitude
        {
            get => (double) GetValue(MaxLatitudeProperty);
            set => SetValue(MaxLatitudeProperty, value);
        }

        /// <summary>
        ///     Optional minimum longitude value. Default is NaN.
        /// </summary>
        public double MinLongitude
        {
            get => (double) GetValue(MinLongitudeProperty);
            set => SetValue(MinLongitudeProperty, value);
        }

        /// <summary>
        ///     Optional maximum longitude value. Default is NaN.
        /// </summary>
        public double MaxLongitude
        {
            get => (double) GetValue(MaxLongitudeProperty);
            set => SetValue(MaxLongitudeProperty, value);
        }

        /// <summary>
        ///     Optional maximum width of the map image's bounding box. Default is NaN.
        /// </summary>
        public double MaxBoundingBoxWidth
        {
            get => (double) GetValue(MaxBoundingBoxWidthProperty);
            set => SetValue(MaxBoundingBoxWidthProperty, value);
        }

        /// <summary>
        ///     Relative size of the map image in relation to the current viewport size.
        ///     Setting a value greater than one will let MapImageLayer request images that
        ///     are larger than the viewport, in order to support smooth panning.
        /// </summary>
        public double RelativeImageSize
        {
            get => (double) GetValue(RelativeImageSizeProperty);
            set => SetValue(RelativeImageSizeProperty, value);
        }

        /// <summary>
        ///     Minimum time interval between images updates.
        /// </summary>
        public TimeSpan UpdateInterval
        {
            get => (TimeSpan) GetValue(UpdateIntervalProperty);
            set => SetValue(UpdateIntervalProperty, value);
        }

        /// <summary>
        ///     Controls if images are updated while the viewport is still changing.
        /// </summary>
        public bool UpdateWhileViewportChanging
        {
            get => (bool) GetValue(UpdateWhileViewportChangingProperty);
            set => SetValue(UpdateWhileViewportChangingProperty, value);
        }

        /// <summary>
        ///     Description of the MapImageLayer.
        ///     Used to display copyright information on top of the map.
        /// </summary>
        public string Description
        {
            get => (string) GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        /// <summary>
        ///     Optional foreground brush.
        ///     Sets MapBase.Foreground if not null and the MapImageLayer is the base map layer.
        /// </summary>
        public Brush MapForeground
        {
            get => (Brush) GetValue(MapForegroundProperty);
            set => SetValue(MapForegroundProperty, value);
        }

        /// <summary>
        ///     Optional background brush.
        ///     Sets MapBase.Background if not null and the MapImageLayer is the base map layer.
        /// </summary>
        public Brush MapBackground
        {
            get => (Brush) GetValue(MapBackgroundProperty);
            set => SetValue(MapBackgroundProperty, value);
        }

        /// <summary>
        ///     Returns an ImageSource for the specified bounding box.
        /// </summary>
        protected abstract Task<ImageSource> GetImageAsync(BoundingBox boundingBox);

        protected override async void OnViewportChanged(ViewportChangedEventArgs e)
        {
            if (e.ProjectionChanged)
            {
                ClearImages();

                base.OnViewportChanged(e);

                await UpdateImageAsync();
            }
            else
            {
                AdjustBoundingBox(e.LongitudeOffset);

                base.OnViewportChanged(e);

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

        protected virtual async Task UpdateImageAsync()
        {
            _updateTimer.Stop();


            if (_updateInProgress)
            {
                _updateTimer.Start(); // update image on next timer tick
                return;
            }

            var parentMap = GetParentMap();
            if (parentMap != null && parentMap.RenderSize.Width > 0 && parentMap.RenderSize.Height > 0)
            {
                _updateInProgress = true;

                UpdateBoundingBox();

                ImageSource imageSource = null;

                if (_boundingBox != null)
                {
                    try
                    {
                        imageSource = await GetImageAsync(_boundingBox);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("MapImageLayer: " + ex.Message);
                    }
                }

                SwapImages(imageSource);

                _updateInProgress = false;
            }
        }

        private void UpdateBoundingBox()
        {
            var parentMap = GetParentMap();
            var width = parentMap.RenderSize.Width * RelativeImageSize;
            var height = parentMap.RenderSize.Height * RelativeImageSize;
            var x = (parentMap.RenderSize.Width - width) / 2d;
            var y = (parentMap.RenderSize.Height - height) / 2d;
            var rect = new Rect(x, y, width, height);

            _boundingBox = parentMap.MapProjection.ViewportRectToBoundingBox(rect);

            if (_boundingBox != null)
            {
                if (!double.IsNaN(MinLatitude) && _boundingBox.South < MinLatitude)
                {
                    _boundingBox.South = MinLatitude;
                }

                if (!double.IsNaN(MinLongitude) && _boundingBox.West < MinLongitude)
                {
                    _boundingBox.West = MinLongitude;
                }

                if (!double.IsNaN(MaxLatitude) && _boundingBox.North > MaxLatitude)
                {
                    _boundingBox.North = MaxLatitude;
                }

                if (!double.IsNaN(MaxLongitude) && _boundingBox.East > MaxLongitude)
                {
                    _boundingBox.East = MaxLongitude;
                }

                if (!double.IsNaN(MaxBoundingBoxWidth) && _boundingBox.Width > MaxBoundingBoxWidth)
                {
                    var d = (_boundingBox.Width - MaxBoundingBoxWidth) / 2d;
                    _boundingBox.West += d;
                    _boundingBox.East -= d;
                }
            }
        }

        private void AdjustBoundingBox(double longitudeOffset)
        {
            if (Math.Abs(longitudeOffset) > 180d && _boundingBox != null)
            {
                var offset = 360d * Math.Sign(longitudeOffset);

                _boundingBox.West += offset;
                _boundingBox.East += offset;

                foreach (var element in Children.OfType<FrameworkElement>())
                {
                    var boundingBox = GetBoundingBox(element);

                    if (boundingBox != null)
                    {
                        SetBoundingBox(element,
                            new BoundingBox(boundingBox.South, boundingBox.West + offset, boundingBox.North, boundingBox.East + offset));
                    }
                }
            }
        }

        private void ClearImages()
        {
            foreach (var element in Children.OfType<FrameworkElement>())
            {
                element.ClearValue(BoundingBoxProperty);
                element.ClearValue(Image.SourceProperty);
            }
        }

        private void SwapImages(ImageSource imageSource)
        {
            var topImage = (Image) Children[0];
            var bottomImage = (Image) Children[1];

            Children.RemoveAt(0);
            Children.Insert(1, topImage);

            topImage.Source = imageSource;
            SetBoundingBox(topImage, _boundingBox?.Clone());

            topImage.BeginAnimation(OpacityProperty, new DoubleAnimation
            {
                To = 1d,
                Duration = Tile.FadeDuration
            });

            bottomImage.BeginAnimation(OpacityProperty, new DoubleAnimation
            {
                To = 0d,
                BeginTime = Tile.FadeDuration,
                Duration = TimeSpan.Zero
            });
        }
    }
}