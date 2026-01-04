using loko.Services;
using loko.Services.Implementations;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using NetTopologySuite.Geometries;
using System.ComponentModel;

namespace loko.Views;

public partial class DownloadMapPage : ContentPage
{

    private WritableLayer _rectangleLayer;
    //private NetTopologySuite.Geometries.Polygon _rectangleGeometry;
    private NetTopologySuite.Geometries.Polygon _rectangleGeometry;
    private LineString _rectangleOutline;
    private const float RectangleOpacity = 0.5f; // Fixed low opacity
    private RasterizingLayer _dimmingLayer;
    private int _minZoomLevel = 1;
    private int _maxZoomLevel = 17;

    //private readonly IServiceProvider _serviceProvider;
    private Mapsui.Map _map;
    private MemoryLayer _markerLayer;
    public DownloadMapPage()
    {
        InitializeComponent();

        //_serviceProvider = serviceProvider;
        InitializeMap();
    }

    private void InitializeMap()
    {
        _map = new Mapsui.Map();
        var osmLayer = OpenStreetMap.CreateTileLayer();
        _map.Layers.Add(osmLayer);

        _rectangleLayer = new WritableLayer { Name = "RectangleLayer" };
        _map.Layers.Add(_rectangleLayer);

        //_dimmingLayer = new RasterizingLayer(CreateDimmingLayer(), 1024) { Name = "DimmingLayer" };
        //map.Layers.Add(_dimmingLayer);

        _markerLayer = new MemoryLayer { Name = "MarkerLayer" };
        _map.Layers.Add(_markerLayer);

        MapControl.Map = _map;
        _map.Navigator.RotationLock = true;
        _map.Navigator.RotateTo(0);

        // Center on a default location (e.g., Rome, Italy)
        var (centerX, centerY) = SphericalMercator.FromLonLat(39, 49);
        _map.Navigator.CenterOn(centerX, centerY);
        _map.Navigator.ZoomTo(10); // Adjust zoom level as needed
        _map.RefreshGraphics();

        UpdateRectanglePosition();


        //Subscribe to the ViewportChanged event
       _map.Navigator.ViewportChanged += Navigator_ViewportChanged;
    }

    private void Navigator_ViewportChanged(object sender, PropertyChangedEventArgs e)
    {
        UpdateRectanglePosition();
    }

    private void UpdateRectanglePosition()
    {
        var viewport = MapControl.Map.Navigator.Viewport;
        var extent = viewport.ToExtent();

        var width = extent.Width * 0.7;
        var height = extent.Height * 0.7;

        var minX = extent.Centroid.X - width / 2;
        var minY = extent.Centroid.Y - height / 2;
        var maxX = extent.Centroid.X + width / 2;
        var maxY = extent.Centroid.Y + height / 2;

        var factory = new GeometryFactory();
        var shell = factory.CreateLinearRing(new Coordinate[]
        {
        new(minX, minY),
        new(maxX, minY),
        new(maxX, maxY),
        new(minX, maxY),
        new(minX, minY)
        });
        _rectangleGeometry = factory.CreatePolygon(shell);
        _rectangleOutline = factory.CreateLineString(shell.Coordinates);

        var viewportPolygon = factory.CreatePolygon(new Coordinate[]
            {
                new(extent.MinX, extent.MinY),
                new(extent.MaxX, extent.MinY),
                new(extent.MaxX, extent.MaxY),
                new(extent.MinX, extent.MaxY),
                new(extent.MinX, extent.MinY)
            });
        var dimmingPolygon = viewportPolygon.Difference(_rectangleGeometry);

        _rectangleLayer.Clear();


        var rectangleFeature = new GeometryFeature(_rectangleOutline);
        UpdateRectangleStyle(rectangleFeature);
        _rectangleLayer.Add(rectangleFeature);

        //(_dimmingLayer.SourceLayer as Layer).Extent = extent.ToExtent();
        //_dimmingLayer.Refresh();

        //var feature = new GeometryFeature(_rectangleOutline);
        //UpdateRectangleStyle(feature);
        //_rectangleLayer.Add(feature);

        MapControl.RefreshGraphics();
    }


    private void UpdateRectangleStyle(GeometryFeature feature)
    {
        var style = new VectorStyle
        {
            Fill = null, // No fill
            Line = new Pen
            {
                Color = Mapsui.Styles.Color.Blue,
                Width = 2,
                PenStrokeCap = PenStrokeCap.Round
            }
        };

        feature.Styles.Clear();
        feature.Styles.Add(style);
    }

    private (double minLon, double minLat, double maxLon, double maxLat) GetRectangleCoordinates()
    {
        var envelope = _rectangleGeometry.EnvelopeInternal;

        // Convert from web mercator to latitude/longitude
        var minLonLat = SphericalMercator.ToLonLat(envelope.MinX, envelope.MinY);
        var maxLonLat = SphericalMercator.ToLonLat(envelope.MaxX, envelope.MaxY);

        return (minLonLat.lon, minLonLat.lat, maxLonLat.lon, maxLonLat.lat);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (MapControl?.Map?.Navigator != null)
        {
            MapControl.Map.Navigator.ViewportChanged -= Navigator_ViewportChanged;
        }
    }

    private void PrintStoragePaths()
    {
        var appDataPath = FileSystem.AppDataDirectory;
        var dbPath = Path.Combine(appDataPath, "AreasDB.db3");
        var tileCachePath = Path.Combine(appDataPath, "MapTileCache");

    }

    private async void OnDownloadClicked(object sender, EventArgs e)
    {
        PrintStoragePaths();
        
        var name = await DisplayPromptAsync("Download Area", "Enter a name for this area:");
        if (string.IsNullOrEmpty(name))
            return;
        var tileDownloader = new TileDownloader(name);
        //var name = "smth";

        var envelope = _rectangleGeometry.EnvelopeInternal;

        // Convert from web mercator to latitude/longitude
        var minLonLat = SphericalMercator.ToLonLat(envelope.MinX, envelope.MinY);
        var maxLonLat = SphericalMercator.ToLonLat(envelope.MaxX, envelope.MaxY);

        var (totalTiles, estimatedSizeMB) = tileDownloader.CalculateTotalTilesAndSize(minLonLat.lon, minLonLat.lat, maxLonLat.lon, maxLonLat.lat, _minZoomLevel, _maxZoomLevel);

        // Ask for confirmation with the estimated size
        bool userConfirmed = await DisplayAlert("Confirm Download",
            $"This will download approximately {estimatedSizeMB:F2} MB ({totalTiles} tiles). Do you want to proceed?",
            "Yes", "No");

        if (!userConfirmed)
            return;

        DownloadProgressBar.IsVisible = true;
        ProgressLabel.IsVisible = true;

        var progress = new Progress<double>(p =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DownloadProgressBar.Progress = p;
            });
        });

        await tileDownloader.DownloadArea(name,minLonLat.lon, minLonLat.lat, maxLonLat.lon, maxLonLat.lat, _minZoomLevel, _maxZoomLevel, totalTiles, progress);

        var downloadedArea = new DownloadedArea
        {
            Name = name,
            MinLon = minLonLat.lon,
            MinLat = minLonLat.lat,
            MaxLon = maxLonLat.lon,
            MaxLat = maxLonLat.lat,
            DownloadSize = estimatedSizeMB,
            MinZoom = _minZoomLevel,
            MaxZoom = _maxZoomLevel,
            DownloadDate = DateTime.Now
        };

        var downloadManage = new DownloadedAreaManager();

        await downloadManage.SaveAreaAsync(downloadedArea);

        DownloadProgressBar.IsVisible = false;
        ProgressLabel.IsVisible = false;


        await DisplayAlert("Download Complete", $"Area '{name}' has been downloaded for offline use.", "OK");


    }



}