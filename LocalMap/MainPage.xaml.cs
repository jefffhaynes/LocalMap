using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using LocalMap;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LocalMap
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var dataSource =
                new CustomMapTileDataSource();
            dataSource.BitmapRequested += DataSourceOnBitmapRequested;

            var source = new MapTileSource(dataSource)
            {
                Layer = MapTileLayer.BackgroundReplacement, TilePixelSize = 512
            };

            Map.Style = MapStyle.None;
            Map.TileSources.Add(source);

            await Geolocator.RequestAccessAsync();

            var geo = new Geolocator();
            var location = await geo.GetGeopositionAsync();

            Map.Center = location.Coordinate.Point;
            Map.ZoomLevel = 10;
        }


        private async void DataSourceOnBitmapRequested(CustomMapTileDataSource sender, MapTileBitmapRequestedEventArgs args)
        {
            var deferral = args.Request.GetDeferral();

            using (var stream = await DatabaseTileImageLoader.LoadTileAsync(new Tile(args.X, args.Y, args.ZoomLevel)))
            {
                if (stream != null)
                {
                    var decoder = await BitmapDecoder.CreateAsync(BitmapDecoder.PngDecoderId, stream);
                    var bitmap = await decoder.GetSoftwareBitmapAsync();
                    var convertedBitmap = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Rgba8);

                    var recodeStream = new InMemoryRandomAccessStream();

                    byte[] imageBytes = new byte[4 * decoder.PixelWidth * decoder.PixelHeight];
                    convertedBitmap.CopyToBuffer(imageBytes.AsBuffer());
                    var writer2 = new DataWriter(recodeStream);
                    writer2.WriteBytes(imageBytes);
                    await writer2.StoreAsync();
                    await writer2.FlushAsync();

                    var streamReference = RandomAccessStreamReference.CreateFromStream(recodeStream);

                    args.Request.PixelData = streamReference;
                }
            }

            deferral.Complete();
        }
    }
}
