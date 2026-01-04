using BruTile;
using BruTile.Cache;
using BruTile.Predefined;
using BruTile.Web;
using loko.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loko.Helpers
{
    public class CustomTileSource : ITileSource
    {
        private readonly FileCache _fileCache;
        private readonly HttpTileSource _onlineSource;
        private readonly DownloadedAreaManager _downloadedAreaManager;
        private double _currentLat;
        private double _currentLon;

        public CustomTileSource(string cacheDirectory, double currentLat, double currentLon)
        {
            _fileCache = new FileCache(cacheDirectory, "png");
            _onlineSource = new HttpTileSource(new GlobalSphericalMercator(),
                "https://tile.openstreetmap.org/{z}/{x}/{y}.png",
                name: "OpenStreetMap",
                attribution: new Attribution("© OpenStreetMap contributors"));
            _downloadedAreaManager = new DownloadedAreaManager();
            _currentLat = currentLat;
            _currentLon = currentLon;
        }

        public void UpdateLocation(double lat, double lon)
        {
            _currentLat = lat;
            _currentLon = lon;
        }

        public ITileSchema Schema => _onlineSource.Schema;

        public string Name => "Custom Tile Source";

        public Attribution Attribution => _onlineSource.Attribution;

        public async Task<byte[]> GetTileAsync(TileInfo tileInfo)
        {
            if (NetworkHelper.IsNetworkAvailable())
            {
                // If internet is available, always try to get the tile from online source
                try
                {
                    var onlineTile = await _onlineSource.GetTileAsync(tileInfo);
                    // Cache the tile for future offline use
                    _fileCache.Add(tileInfo.Index, onlineTile);
                    return onlineTile;
                }
                catch
                {
                    // If fetching online tile fails, fall back to cached tile
                    return await GetCachedTileAsync(tileInfo);
                }
            }
            else
            {
                // If no internet, try to get the tile from cache
                return await GetCachedTileAsync(tileInfo);
            }
        }

        private async Task<byte[]> GetCachedTileAsync(TileInfo tileInfo)
        {
            // First, check if the tile is in the cache
            var cachedTile = _fileCache.Find(tileInfo.Index);
            if (cachedTile != null)
            {
                //Console.WriteLine($"Cached tile found for Level: {tileInfo.Index.Level}, Col: {tileInfo.Index.Col}, Row: {tileInfo.Index.Row}");
                return cachedTile;
            }

            // If not in cache, check if it's within a downloaded area
            try
            {
                var downloadedAreas = await _downloadedAreaManager.GetAreasAsync();
       
                var downloadedArea = await IsTileInDownloadedArea(tileInfo, downloadedAreas);
                if (downloadedArea != null)
                {
                    // The tile is within a downloaded area, so we need to retrieve it from the offline data
                    return await GetTileFromOfflineData(tileInfo, downloadedArea);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCachedTileAsync: {ex.Message}");
            }

            Console.WriteLine($"No tile found for Level: {tileInfo.Index.Level}, Col: {tileInfo.Index.Col}, Row: {tileInfo.Index.Row}");
            return null;
        }

        private async Task<DownloadedArea> IsTileInDownloadedArea(TileInfo tileInfo, List<DownloadedArea> downloadedAreas)
        {

            //var (lon, lat) = TileToWorldPos(tileInfo.Index.Col, tileInfo.Index.Row, tileInfo.Index.Level);
            //foreach (var area in downloadedAreas)
            //{
            //    if (lon >= area.MinLon && lon <= area.MaxLon &&
            //        lat >= area.MinLat && lat <= area.MaxLat &&
            //        tileInfo.Index.Level >= area.MinZoom && tileInfo.Index.Level <= area.MaxZoom)
            //    {
            //        Console.WriteLine($"Tile in downloaded area: {area.Name}");
            //        return area;
            //    }
            //}
            //Console.WriteLine("Tile not in any downloaded area");
            //return null;


            var (lon, lat) = TileToWorldPos(tileInfo.Index.Col, tileInfo.Index.Row, tileInfo.Index.Level);
            foreach (var area in downloadedAreas)
            {
                    if (lon >= area.MinLon && lon <= area.MaxLon &&
                        lat >= area.MinLat && lat <= area.MaxLat)
                    {
                        return area;
                    }
            }
            return null;
        }

        private async Task<byte[]> GetTileFromOfflineData(TileInfo tileInfo, DownloadedArea area)
        {
            var offlineTilePath = ConstructOfflineTilePath(tileInfo, area);
            var directory = Path.GetDirectoryName(offlineTilePath);

            if (!Directory.Exists(directory))
            {
                
                return null;
            }

            if (File.Exists(offlineTilePath))
            {
                // Read and return the tile data
                return await File.ReadAllBytesAsync(offlineTilePath);
            }

            return null;

        }


        private string ConstructOfflineTilePath(TileInfo tileInfo, DownloadedArea area)
        {
            var basePath = Path.Combine(FileSystem.AppDataDirectory, "MapTileCache", area.Name);
            var fullPath = Path.Combine(basePath, $"{tileInfo.Index.Level}", $"{tileInfo.Index.Col}", $"{tileInfo.Index.Row}.png");
            Console.WriteLine($"Attempting to access tile at: {fullPath}");
            return fullPath;
        }

        private (double lon, double lat) TileToWorldPos(int tileX, int tileY, int zoom)
        {
            double n = Math.Pow(2, zoom);
            double lon = tileX / n * 360.0 - 180.0;
            double lat = Math.Atan(Math.Sinh(Math.PI * (1 - 2 * tileY / n))) * 180.0 / Math.PI;
            return (lon, lat);
        }
    }
}
