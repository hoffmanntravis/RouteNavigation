using Dapper;
using Dapper.FluentMap.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Linq;
using System.Web;

namespace RouteNavigation
{
    public class Location
    {
        public Coordinates coordinates = new Coordinates();
        public cartesianCoordinates cartesianCoordinates = new cartesianCoordinates();

        public Vehicle assignedVehicle;
        public int clientPriority;
        public int routeId;
        public int id;
        public int oilPickupSchedule;
        public int vehicleSize = int.MaxValue;
        public string account;
        public string address;
        public string contactName;
        public string contactEmail;
        public double oilTankSize = 0;
        public double daysUntilDue = double.NaN;
        public double distanceFromDepot;
        public TimeSpan greaseTrapPreferredTimeStart = Config.Calculation.workdayStartTime;
        public TimeSpan greaseTrapPreferredTimeEnd = Config.Calculation.workdayEndTime;
        public bool oilPickupCustomer;
        public bool greaseTrapCustomer;
        public TimeSpan projectedAmountOverdue;

        public DateTime lastVisited = default(DateTime);
        public double currentGallonsEstimate;
        public DateTime oilPickupNextDate;
        public List<Location> neighbors  = new List<Location>();
    }

    public class Coordinates
    {
        public double lat { get; set; } = Double.NaN;
        public double lng { get; set; } = Double.NaN;
    }

    public class cartesianCoordinates
    {
        public double x { get; set; } = Double.NaN;
        public double y { get; set; } = Double.NaN;
        public double z { get; set; } = Double.NaN;
    }

    public class CoordinatesMap : EntityMap<Coordinates>
    {
        public CoordinatesMap()
        {
            Map(c => c.lat).ToColumn("coordinates_latitude");
            Map(c => c.lng).ToColumn("coordinates_longitude");
        }
    }

    public class sphericalCoordinatesMap : EntityMap<cartesianCoordinates>
    {
        public sphericalCoordinatesMap()
        {
            Map(c => c.x).ToColumn("cartesian_x");
            Map(c => c.y).ToColumn("cartesian_y");
            Map(c => c.z).ToColumn("cartesian_z");
        }
    }
}