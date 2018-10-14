using System;
using System.Collections.Generic;
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
        public DateTime pickupWindowStartTime = Config.Calculation.workdayStartTime;
        public DateTime pickupWindowEndTime = Config.Calculation.workdayStartTime;
        public string type;
        

        public DateTime lastVisited = default(DateTime);
        public double currentGallonsEstimate;
        public TimeSpan visitTime;
        public List<Location> neighbors = new List<Location>();
    }

    public class Coordinates
    {
        public double lat = Double.NaN;
        public double lng = Double.NaN;
    }

}