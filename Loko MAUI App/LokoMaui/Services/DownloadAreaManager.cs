using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loko.Services
{
    public class DownloadedAreaManager
    {
        private readonly string _dbPath;
        private SQLiteAsyncConnection _database;

        public DownloadedAreaManager()
        {
            _dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AreasDB.db3");
            _database = new SQLiteAsyncConnection(_dbPath);
            _database.CreateTableAsync<DownloadedArea>().Wait();
        }

        public Task<List<DownloadedArea>> GetAreasAsync()
        {
            return _database.Table<DownloadedArea>().ToListAsync();
        }

        public Task<DownloadedArea> GetAreaAsync(string name)
        {
            return _database.Table<DownloadedArea>()
                .Where(i => i.Name == name)
                .FirstOrDefaultAsync();
        }

        public Task<int> SaveAreaAsync(DownloadedArea area)
        {
            return _database.InsertAsync(area);
        }

        public async Task<bool> DeleteAreaAsync(string name)
        {
            try
            {
                // Get the area to be deleted
                var area = await GetAreaAsync(name);
                if (area == null)
                {
                    return false; // Area not found
                }

                // Delete the database record
                await _database.DeleteAsync<DownloadedArea>(area.Id);

                // Delete the cached tiles
                await DeleteCachedTiles(area);

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error deleting area: {ex.Message}");
                return false;
            }
        }

        public async Task DeleteCachedTiles(DownloadedArea area)
        {
            var cachePath = Path.Combine(FileSystem.AppDataDirectory, "MapTileCache", area.Name);

            for (int zoom = area.MinZoom; zoom <= area.MaxZoom; zoom++)
            {
                var (minTileX, minTileY) = LatLonToTileXY(area.MaxLat, area.MinLon, zoom);
                var (maxTileX, maxTileY) = LatLonToTileXY(area.MinLat, area.MaxLon, zoom);

                for (int x = minTileX; x <= maxTileX; x++)
                {
                    for (int y = minTileY; y <= maxTileY; y++)
                    {
                        var tilePath = Path.Combine(cachePath, $"{zoom}", $"{x}", $"{y}.png");
                        if (File.Exists(tilePath))
                        {
                            File.Delete(tilePath);
                        }
                    }
                }
            }

            // Clean up empty directories
            CleanupEmptyDirectories(cachePath);
        }

        private (int, int) LatLonToTileXY(double lat, double lon, int zoom)
        {
            var n = Math.Pow(2, zoom);
            var x = (int)((lon + 180.0) / 360.0 * n);
            var y = (int)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) +
                1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * n);
            return (x, y);
        }

        private void CleanupEmptyDirectories(string startPath)
        {
            foreach (var directory in Directory.GetDirectories(startPath))
            {
                CleanupEmptyDirectories(directory);
                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory, false);
                }
            }
        }
    }

    public class DownloadedArea
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public double MinLon { get; set; }
        public double MinLat { get; set; }
        public double MaxLon { get; set; }
        public double MaxLat { get; set; }
        public double DownloadSize { get; set; }
        public int MinZoom { get; set; }
        public int MaxZoom { get; set; }
        public DateTime DownloadDate { get; set; }
    }
}
