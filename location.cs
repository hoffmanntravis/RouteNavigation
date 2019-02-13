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
        public Vehicle assignedVehicle;
        public int clientPriority;
        public int routeId;
        public int id;
        public int pickupIntervalDays;
        public int vehicleSize = int.MaxValue;
        public string locationName;
        public string address;
        public string contactName;
        public string contactEmail;
        public double capacityGallons = 0;
        public double daysUntilDue = double.NaN;
        public double distanceFromDepot;
        public TimeSpan pickupWindowStartTime = Config.Calculation.workdayStartTime;
        public TimeSpan pickupWindowEndTime = Config.Calculation.workdayEndTime;
        public bool hasOil;
        public bool hasGrease;
        public TimeSpan projectedAmountOverdue;

        public DateTime lastVisited = default(DateTime);
        public double currentGallonsEstimate;
        public DateTime intendedPickupDate;
        public List<Location> neighbors  = new List<Location>();
    }

    public class Coordinates
    {
        public double lat { get; set; } = Double.NaN ;
        public double lng { get; set; } = Double.NaN;
    }

    public class CoordinatesMap : EntityMap<Coordinates>
    {
        public CoordinatesMap()
        {
            Map(c => c.lat).ToColumn("coordinates_latitude");
            Map(c => c.lng).ToColumn("coordinates_longitude");
        }
    }

}