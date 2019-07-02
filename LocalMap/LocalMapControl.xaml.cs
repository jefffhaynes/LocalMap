using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LocalMap
{
    public sealed partial class LocalMapControl : UserControl
    {
        float displayDpi;


        public LocalMapControl()
        {
            this.InitializeComponent();
            canvas.Width = TileSize * 2;
            canvas.Height = TileSize * 2;
        }

        private const int TileSize = 512;
        private float Zoom => (float) Math.Log(scrollViewer.ZoomFactor, 2);

        private int TilesAcross => (int)Math.Pow(2, (int)Zoom);


        private async void Canvas_OnRegionsInvalidated(CanvasVirtualControl sender, CanvasRegionsInvalidatedEventArgs args)
        {
            var zoomLevel = (int) Zoom;

            ////const float baseScale = 0.005f;
            //const float baseScale = 1f;
            //float scale = baseScale * 96 / sender.Dpi;
            //Vector2 translate = baseScale * sender.Size.ToVector2();
            var scale = 1 / Math.Pow(2, zoomLevel - 1);

            var tileSize = TileSize * scale;

            foreach (var argsInvalidatedRegion in args.InvalidatedRegions)
            {
                var left = (int)Math.Max(argsInvalidatedRegion.Left / tileSize, 0);
                var right = (int)Math.Ceiling(argsInvalidatedRegion.Right / tileSize);
                var top = (int)Math.Max(argsInvalidatedRegion.Top / tileSize, 0);
                var bottom = (int)Math.Ceiling(argsInvalidatedRegion.Bottom / tileSize);

                var max = TilesAcross - 1;

                left = Math.Clamp(left, 0, max);
                right = Math.Clamp(right, 0, max);
                top = Math.Clamp(top, 0, max);
                bottom = Math.Clamp(bottom, 0, max);

                var tasks = new List<Task>();

                for (int x = left; x <= right; x++)
                {
                    for (int y = top; y <= bottom; y++)
                    {
                        var task =  DrawTileAsync(sender, x, y, scale, tileSize, zoomLevel);
                        tasks.Add(task);
                    }
                }

                await Task.WhenAll(tasks);
            }
        }

        private static async Task DrawTileAsync(CanvasVirtualControl sender, int x, int y, double scale, double tileSize,
            int zoomLevel)
        {
            var rect = new Rect(x * tileSize, y * tileSize, tileSize, tileSize);

            using (var session = sender.CreateDrawingSession(rect))
            {
                var translation = Matrix3x2.CreateTranslation((float) rect.X, (float) rect.Y);
                var scaling = Matrix3x2.CreateScale((float) scale);
                session.Transform = Matrix3x2.Multiply(scaling, translation);

                var tile = new Tile(x, y, zoomLevel);
                await DatabaseTileImageLoader.DrawTileAsync(tile, session, TileSize);
            }
        }

        private void ScrollViewer_OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // Cancel out the display DPI, so our fractal always renders at 96 DPI regardless of display
            // configuration. This boosts performance on high DPI displays, at the cost of visual quality.
            // For even better performance (but lower quality) this value could be further reduced.
            float dpiAdjustment = 96 / displayDpi;

            // Adjust DPI to match the current zoom level.
            float dpiScale = dpiAdjustment * scrollViewer.ZoomFactor;

            // To boost performance during pinch-zoom manipulations, we only update DPI when it has
            // changed by more than 20%, or at the end of the zoom (when e.IsIntermediate reports false).
            // Smaller changes will just scale the existing bitmap, which is much faster than recomputing
            // the fractal at a different resolution. To trade off between zooming perf vs. smoothness,
            // adjust the thresholds used in this ratio comparison.
            var ratio = canvas.DpiScale / dpiScale;

            if (e == null || !e.IsIntermediate || ratio <= 0.8 || ratio >= 1.25)
            {
                canvas.DpiScale = dpiScale;
            }
        }
        void Display_DpiChanged(DisplayInformation sender, object args)
        {
            displayDpi = sender.LogicalDpi;

            // Manually call the ViewChanged handler to update DpiScale.
            ScrollViewer_OnViewChanged(null, null);
        }

        private void LocalMapControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            // Initialize the display DPI, and listen for events in case this changes.
            var display = DisplayInformation.GetForCurrentView();
            display.DpiChanged += Display_DpiChanged;
            Display_DpiChanged(display, null);
        }
    }
}
