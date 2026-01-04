using loko.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace loko.Services.Implementations
{
    public class DeviceDetailsDB
    {
        public const string DatabaseFilename = "DeviceDetailsSQLite.db3";

        public const SQLite.SQLiteOpenFlags Flags =
            // open the database in read/write mode
            SQLite.SQLiteOpenFlags.ReadWrite |
            // create the database if it doesn't exist
            SQLite.SQLiteOpenFlags.Create |
            // enable multi-threaded database access
            SQLite.SQLiteOpenFlags.SharedCache;

        public static string DatabasePath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, DatabaseFilename);
            }
        }
        static SQLiteAsyncConnection Database;

        public static readonly AsyncLazy<DeviceDetailsDB> Instance = new(async () =>
        {
            var instance = new DeviceDetailsDB();
            CreateTableResult result = await Database.CreateTableAsync<DBDeviceDetails>();
            return instance;
        });

        public DeviceDetailsDB()
        {
            Database = new SQLiteAsyncConnection(DatabasePath, Flags);
        }

        public Task<int> SaveRecordToBaseAsync(BLEDeviceDetails record)
        {
            var dbRecord = GetDBDeviceDetails(record);
            return Database.InsertAsync(dbRecord);
        }

        public async Task<List<BLEDeviceDetails>> GetRecordsByDeviceId(int deviceId)
        {
            var records = new List<BLEDeviceDetails>();
            var dbRecords = await Database.Table<DBDeviceDetails>().Where(x => x.DateTime >= DateTime.Today).Where(x => x.DeviceId == deviceId).ToListAsync();
            if (dbRecords.Any())
                foreach (var dbRecord in dbRecords)
                {
                    var record = GetBLEDeviceDetails(dbRecord);
                    records.Add(record);
                }
            return records;
        }

        public async Task<List<string>> GetDatesFromRecords()
        {
            var dates = new List<string>();
            var dbRecords = await Database.Table<DBDeviceDetails>().ToListAsync();
            foreach (var dbRecord in dbRecords)
            {
                if (!dates.Contains(dbRecord.DateTime.ToString("MM'/'dd'/'yyyy")))
                    dates.Add(dbRecord.DateTime.ToString("MM'/'dd'/'yyyy"));
            }
            return dates;
        }

        public async Task<List<BLEDeviceDetails>> GetRecordsByDate(string date)
        {
            var records = new List<BLEDeviceDetails>();
            var selectedDate = DateTime.ParseExact(date, "MM'/'dd'/'yyyy", System.Globalization.CultureInfo.InvariantCulture);
            var selectedDateEnd = selectedDate.AddDays(1);
            var dbRecords = await Database.Table<DBDeviceDetails>().Where(
                x => x.DateTime > selectedDate && x.DateTime < selectedDateEnd).ToListAsync();
            if (dbRecords.Any())
                foreach (var dbRecord in dbRecords)
                {
                    var record = GetBLEDeviceDetails(dbRecord);
                    records.Add(record);
                }
            return records;
        }

        public async Task<List<DeviceModel>> GetDeviceModelsFromRecords()
        {
            var devices = new List<DeviceModel>();
            var dbRecords = await Database.Table<DBDeviceDetails>().Where(x => x.DateTime >= DateTime.Today).ToListAsync();
            var t = dbRecords?.Select(x => x.DeviceId).Distinct();
            if (dbRecords?.Any() == true)
                foreach (var dbRecord in dbRecords)
                {
                    if (!devices.Any(x => x.Label == dbRecord.DeviceId.ToString()))
                        devices.Add(new DeviceModel { Label = dbRecord.DeviceId.ToString() });
                }
            return devices;
        }

        public Task<int> ClearDB()
        {
            return Database.DeleteAllAsync<DBDeviceDetails>();
        }

        public async Task DeleteRecordsByDate(string date)
        {
            var selectedDate = DateTime.ParseExact(date, "MM'/'dd'/'yyyy", System.Globalization.CultureInfo.InvariantCulture);
            var nextDay = selectedDate.AddDays(1);
            var t = await Database.Table<DBDeviceDetails>().Where(x=> x.DateTime >= selectedDate && x.DateTime < nextDay).DeleteAsync();            
        }

        private DBDeviceDetails GetDBDeviceDetails(BLEDeviceDetails deviceInfo)
        {
            DBDeviceDetails dbDeviceDetails = new()
            {
                DeviceId = deviceInfo.DeviceId,
                Latitude = deviceInfo.Latitude,
                Longitude = deviceInfo.Longitude,
                BatteryLevel = deviceInfo.BatteryLevel,
                DateTime = deviceInfo.DateTime
            };

            return dbDeviceDetails;
        }

        private BLEDeviceDetails GetBLEDeviceDetails(DBDeviceDetails deviceInfo)
        {
            BLEDeviceDetails bleDeviceDetails = new()
            {
                DeviceId = deviceInfo.DeviceId,
                Latitude = deviceInfo.Latitude,
                Longitude = deviceInfo.Longitude,
                BatteryLevel = deviceInfo.BatteryLevel,
                DateTime = deviceInfo.DateTime
            };
            return bleDeviceDetails;
        }
    }

    public class AsyncLazy<T>
    {
        readonly Lazy<Task<T>> instance;

        public AsyncLazy(Func<T> factory)
        {
            instance = new Lazy<Task<T>>(() => Task.Run(factory));
        }

        public AsyncLazy(Func<Task<T>> factory)
        {
            instance = new Lazy<Task<T>>(() => Task.Run(factory));
        }

        public TaskAwaiter<T> GetAwaiter()
        {
            return instance.Value.GetAwaiter();
        }
    }
}
