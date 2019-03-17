// XAML Map Control - https://github.com/ClemensFischer/XAML-Map-Control
// © 2018 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System;
#if WINDOWS_UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
#endif

namespace MapControl
{
    /// <summary>
    /// Draws a map scale overlay.
    /// </summary>
    public class MapScale : MapOverlay
    {
        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(
            nameof(Padding), typeof(Thickness), typeof(MapScale), new PropertyMetadata(new Thickness(4)));

        private readonly Polyline _line = new Polyline();

        private readonly TextBlock _label = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            TextAlignment = TextAlignment.Center
        };

        public MapScale()
        {
            IsHitTestVisible = false;
            MinWidth = 100d;

            _line.SetBinding(Shape.StrokeProperty, GetBinding(StrokeProperty, nameof(Stroke)));
            _line.SetBinding(Shape.StrokeThicknessProperty, GetBinding(StrokeThicknessProperty, nameof(StrokeThickness)));
#if WINDOWS_UWP
            _label.SetBinding(TextBlock.ForegroundProperty, GetBinding(ForegroundProperty, nameof(Foreground)));
#endif
            Children.Add(_line);
            Children.Add(_label);
        }

        public Thickness Padding
        {
            get => (Thickness)GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var size = new Size();

            var parentMap = GetParentMap();
            if (parentMap != null && parentMap.ScaleTransform.ScaleX > 0d)
            {
                var length = MinWidth / parentMap.ScaleTransform.ScaleX;
                var magnitude = Math.Pow(10d, Math.Floor(Math.Log10(length)));

                if (length / magnitude < 2d)
                {
                    length = 2d * magnitude;
                }
                else if (length / magnitude < 5d)
                {
                    length = 5d * magnitude;
                }
                else
                {
                    length = 10d * magnitude;
                }

                size.Width = length * parentMap.ScaleTransform.ScaleX + StrokeThickness + Padding.Left + Padding.Right;
                size.Height = 1.25 * FontSize + StrokeThickness + Padding.Top + Padding.Bottom;

                var x1 = Padding.Left + StrokeThickness / 2d;
                var x2 = size.Width - Padding.Right - StrokeThickness / 2d;
                var y1 = size.Height / 2d;
                var y2 = size.Height - Padding.Bottom - StrokeThickness / 2d;

                _line.Points = new PointCollection
                {
                    new Point(x1, y1),
                    new Point(x1, y2),
                    new Point(x2, y2),
                    new Point(x2, y1)
                };
                _line.Measure(size);

                _label.Text = length >= 1000d ? $"{length / 1000d:0} km" : $"{length:0} m";
                _label.Width = size.Width;
                _label.Height = size.Height;
                _label.Measure(size);
            }

            return size;
        }

        protected override void OnViewportChanged(ViewportChangedEventArgs e)
        {
            InvalidateMeasure();
        }
    }
}
