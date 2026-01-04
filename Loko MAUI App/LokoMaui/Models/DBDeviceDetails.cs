using SQLite;
using System;

namespace loko.Models;

public class DBDeviceDetails
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public DateTime DateTime { get; set; }
    public int DeviceId { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public int BatteryLevel { get; set; }

}
