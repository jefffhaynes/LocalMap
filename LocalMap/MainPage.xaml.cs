using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using LocalMap;
using Mapsui.Layers;
using Mapsui.Utilities;
using BruTile;
using System.Collections.Generic;
using System.IO;
using BruTile.Predefined;
using Mapsui;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LocalMap
{
    class VectorTileSchema : ITileSchema
    {
        public string Name => throw new NotImplementedException();

        public string Srs => throw new NotImplementedException();

        public Extent Extent => new Extent(-180, -85.0511, 180, 85.0511);

        public IDictionary<string, Resolution> Resolutions => new Dictionary<string, Resolution>
        {
            {"something", new Resolution("something", 1, 512, 512) }
        };

        public string Format => throw new NotImplementedException();

        public YAxis YAxis => throw new NotImplementedException();

        public Extent GetExtentOfTilesInView(Extent extent, string levelId)
        {
            throw new NotImplementedException();
        }

        public int GetMatrixFirstCol(string levelId)
        {
            throw new NotImplementedException();
        }

        public int GetMatrixFirstRow(string levelId)
        {
            throw new NotImplementedException();
        }

        public int GetMatrixHeight(string levelId)
        {
            throw new NotImplementedException();
        }

        public int GetMatrixWidth(string levelId)
        {
            throw new NotImplementedException();
        }

        public double GetOriginX(string levelId)
        {
            throw new NotImplementedException();
        }

        public double GetOriginY(string levelId)
        {
            throw new NotImplementedException();
        }

        public int GetTileHeight(string levelId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TileInfo> GetTileInfos(Extent extent, string levelId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TileInfo> GetTileInfos(Extent extent, double unitsPerPixel)
        {
            throw new NotImplementedException();
        }

        public int GetTileWidth(string levelId)
        {
            throw new NotImplementedException();
        }
    }

    public class VectorTileSource : ITileSource
    {
        public VectorTileSource(ITileSchema schema)
        {
            Schema = schema;
        }

        public ITileSchema Schema { get; }

        public string Name => throw new NotImplementedException();

        public Attribution Attribution => new Attribution();

        public byte[] GetTile(TileInfo tileInfo)
        {
            var index = tileInfo.Index;
            var streamTask = DatabaseTileImageLoader.LoadTileAsync(new Tile(index.Col, index.Row, int.Parse(index.Level)));
            streamTask.Wait();
            var stream = streamTask.Result;

            return stream?.AsStream().ToBytes();
        }
    }

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
            Map.Map.Layers.Add(new TileLayer(new VectorTileSource(new GlobalSphericalMercator())));
            //var dataSource =
            //    new CustomMapTileDataSource();
            //dataSource.BitmapRequested += DataSourceOnBitmapRequested;

            //var source = new MapTileSource(dataSource)
            //{
            //    Layer = MapTileLayer.BackgroundReplacement, TilePixelSize = 512
            //};

            //Map.Style = MapStyle.None;
            //Map.TileSources.Add(source);

            //await Geolocator.RequestAccessAsync();

            //var geo = new Geolocator();
            //var location = await geo.GetGeopositionAsync();

            //Map.Center = location.Coordinate.Point;
            //Map.ZoomLevel = 10;
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
