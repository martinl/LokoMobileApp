using BruTile;
using BruTile.Cache;
using BruTile.Predefined;
using BruTile.Web;
using Mapsui.Projections;
using System.Collections.Concurrent;


namespace loko.Services.Implementations
{
    public class TileDownloader
    {
        private readonly ITileSource _tileSource;
        private readonly ITileSchema _schema;
        private readonly FileCache _fileCache;
        private string? _cachePath;
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _throttler;
        private readonly ConcurrentDictionary<string, byte[]> _memoryCache;

        public TileDownloader(string name)
        {
            var cachePath = Path.Combine(FileSystem.AppDataDirectory, "MapTileCache", name);
            _cachePath = cachePath;
            Directory.CreateDirectory(cachePath);
            var url = "https://tile.openstreetmap.org/{z}/{x}/{y}.png";
            var userAgent = "noliLab";

            _httpClient = new HttpClient();
            _fileCache = new FileCache(cachePath, "png");
            _tileSource = CreateTileSource();
    
            _schema = _tileSource.Schema;

            _throttler = new SemaphoreSlim(10, 10);
            _memoryCache = new ConcurrentDictionary<string, byte[]>();
        }

        public static ITileSource CreateTileSource()
        {
            // Define the tile schema
            var schema = new GlobalSphericalMercator();

            // Define the URL template for the tiles
            string urlFormat = "https://tile.openstreetmap.org/{z}/{x}/{y}.png";

            // Create the HttpTileSource (which implements ITileProvider)
            var httpTileSource = new HttpTileSource(
                tileSchema: schema,
                urlFormatter: urlFormat,
                name: "OpenStreetMap",
                attribution: new Attribution("© OpenStreetMap contributors")
            );

            // Create the TileSource using the HttpTileSource and schema
            var tileSource = new TileSource(httpTileSource, schema)
            {
                Name = "OpenStreetMap"
            };

            return tileSource;
        }

        public async Task DownloadArea(string name, double minLon, double minLat, double maxLon, double maxLat, int minZoom, int maxZoom, int totalTiles, IProgress<double> progress = null)
        {
            var tileCount = 0;
            var cachePath = Path.Combine(FileSystem.AppDataDirectory, "MapTileCache", name);
            var tasks = new List<Task>();

            for (var zoom = minZoom; zoom <= maxZoom; zoom++)
            {
                var (minX, minY) = SphericalMercator.FromLonLat(minLon, minLat);
                var (maxX, maxY) = SphericalMercator.FromLonLat(maxLon, maxLat);
                var extent = new Extent(minX, minY, maxX, maxY);
                var tileInfos = _schema.GetTileInfos(extent, zoom);

                foreach (var tileInfo in tileInfos)
                {
                    tasks.Add(DownloadTileAsync(tileInfo, cachePath));
                }

                if (tasks.Count >= 100 || zoom == maxZoom)
                {
                    await Task.WhenAll(tasks);
                    tileCount += tasks.Count;
                    progress?.Report((double)tileCount / totalTiles);
                    tasks.Clear();
                }
            }

            async Task DownloadTileAsync(TileInfo tileInfo, string cachePath)
            {
                await _throttler.WaitAsync();
                try
                {
                    var cacheKey = $"{tileInfo.Index.Level}:{tileInfo.Index.Col}:{tileInfo.Index.Row}";
                    if (!_memoryCache.TryGetValue(cacheKey, out byte[] data))
                    {
                        data = await _tileSource.GetTileAsync(tileInfo);
                        if (data != null)
                        {
                            _memoryCache[cacheKey] = data;
                            await SaveTileAsync(tileInfo, data, cachePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error downloading tile: {ex.Message}");
                }
                finally
                {
                    _throttler.Release();
                }
            }
        }
        private async Task SaveTileAsync(TileInfo? tileInfo, byte[] data, string cachePath)
        {
            var filePath = Path.Combine(cachePath, $"{tileInfo.Index.Level}", $"{tileInfo.Index.Col}", $"{tileInfo.Index.Row}.png");
         
            
            var directory = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllBytesAsync(filePath, data);
            Console.WriteLine($"Saved tile: {filePath}");
        }

        public (int totalTiles, double estimatedSizeMB) CalculateTotalTilesAndSize(double minLon, double minLat, double maxLon, double maxLat, int minZoom, int maxZoom)
        {
            int totalTiles = 0;
            const double averageTileSizeKB = 17.0; // Assume 15KB per tile on average

            for (var zoom = minZoom; zoom <= maxZoom; zoom++)
            {
                try
                {
                    var minTile = SphericalMercator.FromLonLat(minLon, minLat);
                    var maxTile = SphericalMercator.FromLonLat(maxLon, maxLat);
                    var extent = new Extent(minTile.x, minTile.y, maxTile.x, maxTile.y);
                    var tileRange = _schema.GetTileInfos(extent, zoom);
                    totalTiles += tileRange.Count();

                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.Message);
                }
                
            }

            double estimatedSizeMB = (totalTiles * averageTileSizeKB) / 1024.0; // Convert to MB

            return (totalTiles, estimatedSizeMB);
        }
    }
}
